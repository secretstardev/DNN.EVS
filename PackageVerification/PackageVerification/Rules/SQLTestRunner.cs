#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PackageVerification.Data;
using PackageVerification.Models;
using PackageVerification.SQLRunner;
using PackageVerification.SQLRunner.Models;
using PackageVerification.SqlScanner;
using Database = PackageVerification.SQLRunner.Database;
using User = PackageVerification.SQLRunner.User;

namespace PackageVerification.Rules
{
    public class SQLTestRunner:IManifestRule
    {
        public List<VerificationMessage> ApplyRule(Package package, Models.Manifest manifest)
        {
            var r = new List<VerificationMessage>();
            //Trace.WriteLine("SQL Scanner - started", "Information");

            try
            {
                if (manifest.HasSqlScripts)
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    DotNetNukeVersionInfo highestAzureCompatableVersion = null;
                    var majorVersion = DotNetNukeMajorVersions.DotNetNukeVersionList();

                    foreach (var dotNetNukeVersionInfo in majorVersion)
                    {
                        if (DotNetNukeVersions.ConvertNameToRank(package.MinDotNetNukeVersion) <=
                            DotNetNukeVersions.ConvertNameToRank(dotNetNukeVersionInfo.Name))
                        {
                            var databaseName = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                                      .Substring(0, 22)
                                                      .Replace("/", "_")
                                                      .Replace("+", "-");
                            
                            try
                            {
                                //create database
                                //Trace.WriteLine("SQL Scanner - creating database: " + databaseName, "Information");
                                Database.CreateDatabase(databaseName);

                                //create user for the database
                                //Trace.WriteLine("SQL Scanner - creating database user.", "Information");
                                User.CreateDatabaseAdmin(databaseName);

                                //install the base version script
                                //Trace.WriteLine("SQL Scanner - install base DNN db objects.", "Information");
                                DNNBase.InstallBaseDNNScripts(databaseName, dotNetNukeVersionInfo);

                                //list and record all DB objects
                                //Trace.WriteLine("SQL Scanner - build list of base db objects.", "Information");
                                var objectListPre = Objects.GetDatabaseObjects(databaseName);

                                //install extension scripts
                                //Trace.WriteLine("SQL Scanner - install extension.", "Information");
                                foreach (var sqlScriptFile in manifest.InstallScripts)
                                {
                                    if (!File.Exists(sqlScriptFile.TempFilePath) || sqlScriptFile.IsUnInstall) continue;

                                    try
                                    {
                                        Script.Execute(databaseName, File.ReadAllText(sqlScriptFile.TempFilePath));
                                    }
                                    catch (Exception ex)
                                    {
                                        Trace.TraceError("SQL Scanner - There was a problem installing: " + sqlScriptFile.Name);

                                        var error = ex.InnerException.Message;
                                        r.Add(new VerificationMessage
                                            {
                                                Message =
                                                    "While testing against " + dotNetNukeVersionInfo.Name + " " +
                                                    sqlScriptFile.Name + " returned an error: " +
                                                    Common.CleanError(error),
                                                MessageType = MessageTypes.Error,
                                                MessageId = new Guid("b25d95e3-06d0-4241-9729-96f85cfddcbf"),
                                                Rule = GetType().ToString()
                                            });
                                    }
                                }

                                //scan and record all objects created or updated after we installed the extension.
                                //Trace.WriteLine("SQL Scanner - Get new list of db objects post install.", "Information");
                                var objectListPost = Objects.GetDatabaseObjects(databaseName);

                                //check to see if any core objects were modified.
                                //Trace.WriteLine("SQL Scanner - finding objects that might have been updated.", "Information");
                                foreach (var o1 in objectListPre)
                                {
                                    foreach (var o2 in objectListPost)
                                    {
                                        if (o1.ObjectId == o2.ObjectId && o1.ModifyDate != o2.ModifyDate)
                                        {
                                            r.Add(new VerificationMessage()
                                                {
                                                    Message =
                                                        "While testing against " + dotNetNukeVersionInfo.Name +
                                                        " it was detected that a DotNetNuke core SQL object (" + o1.Name +
                                                        ") was modified by this extension.",
                                                    MessageType = MessageTypes.Warning,
                                                    MessageId = new Guid("50072188-f3c3-4ae0-aaed-4f3d22162e9b"),
                                                    Rule = GetType().ToString()
                                                });
                                        }
                                    }
                                }

                                var newObjectList = new List<DatabaseObject>();

                                foreach (var o1 in objectListPost)
                                {
                                    var foundMatch = false;

                                    foreach (var o2 in objectListPre)
                                    {
                                        if (o1.ObjectId == o2.ObjectId)
                                        {
                                            foundMatch = true;
                                        }
                                    }

                                    if (!foundMatch)
                                    {
                                        newObjectList.Add(o1);
                                    }
                                }

                                //check for objects not correctly using {databaseOwner} & {objectQualifier}
                                //Trace.WriteLine("SQL Scanner - Scanning for objects that might not correctly be using dbo & object qualifier tokens.", "Information");
                                foreach (var databaseObject in newObjectList)
                                {
                                    if (databaseObject.SchemaId != 5 && databaseObject.Type.Trim() != "IT")
                                    {
                                        r.Add(new VerificationMessage()
                                            {
                                                Message =
                                                    "While testing against " + dotNetNukeVersionInfo.Name +
                                                    " It appears the extension has created a database object using the incorrect schema: " +
                                                    databaseObject.Name,
                                                MessageType = MessageTypes.Error,
                                                MessageId = new Guid("bd4ca33b-aa33-4d5f-9468-8d8f7491bb9a"),
                                                Rule = GetType().ToString()
                                            });
                                    }

                                    switch (databaseObject.Type.Trim())
                                    {
                                        case "UQ":
                                            if (!databaseObject.Name.StartsWith("IX_TestQualifier_") && !databaseObject.Name.StartsWith("IX__TestQual"))
                                            {
                                                if (databaseObject.Name.Contains("TestQual"))
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a unique constraint using an non-traditional naming convention it should be IX_{objectQualifier}ObjectName: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("fc6b8fa6-2863-4bdf-a55c-a84e8165a57a"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                                else
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a unique constraint which is missing an {objectQualifier} token: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("91b12382-7556-42d0-8772-fb91873232a5"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                            }
                                            break;
                                        case "PK":
                                            if (!databaseObject.Name.StartsWith("PK_TestQualifier_") && !databaseObject.Name.StartsWith("PK__TestQual"))
                                            {
                                                if (databaseObject.Name.Contains("TestQual"))
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a primary key using an non-traditional naming convention it should be PK_{objectQualifier}ObjectName: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("e0bda944-44ab-4338-abb3-8a7bc37c42ee"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                                else
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a primary key which is missing an {objectQualifier} token: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("e25340cb-0e36-4217-a79a-b6ea7faac273"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                            }
                                            break;
                                        case "F":
                                            if (!databaseObject.Name.StartsWith("FK_TestQualifier_") && !databaseObject.Name.StartsWith("FK__TestQual"))
                                            {
                                                if (databaseObject.Name.Contains("TestQual"))
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a foreign key using an non-traditional naming convention it should be FK_{objectQualifier}ObjectName: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("15c557e1-8a94-4aba-aba3-a952de46f9ba"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                                else
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a foreign key which is missing an {objectQualifier} token: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("a5620d04-16cd-467b-8d86-2f400cd04376"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                            }
                                            break;
                                        case "D":
                                            if (!databaseObject.Name.StartsWith("DF_TestQualifier_") && !databaseObject.Name.StartsWith("DF__TestQual"))
                                            {
                                                if (databaseObject.Name.Contains("TestQual"))
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a default constraint using an non-traditional naming convention it should be DF_{objectQualifier}ObjectName: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("15c557e1-8a94-4aba-aba3-a952de46f9ba"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                                else
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a default constraint which is missing an {objectQualifier} token: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("a44d52d4-0e7f-438c-9ae1-539f7251f756"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                            }
                                            break;
                                        case "IT":
                                            break;
                                        default:
                                            if (!databaseObject.Name.StartsWith("TestQualifier_"))
                                            {
                                                if (databaseObject.Name.Contains("TestQual"))
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a database object using an non-traditional naming convention it should be {objectQualifier}ObjectName: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("15c557e1-8a94-4aba-aba3-a952de46f9ba"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                                else
                                                {
                                                    r.Add(new VerificationMessage()
                                                    {
                                                        Message =
                                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                                            " It appears the extension has created a database object which is missing an {objectQualifier} token: " +
                                                            databaseObject.Name,
                                                        MessageType = MessageTypes.Warning,
                                                        MessageId = new Guid("ba2855ee-73a5-4787-911b-426b7c0f24ef"),
                                                        Rule = GetType().ToString()
                                                    });
                                                }
                                            }
                                            break;
                                    }
                                }

                                //run sp_refreshsqlmodule on all newly created stored procedures to check for errors.
                                //Trace.WriteLine("SQL Scanner - Running sp_refreshsqlmodule on newly created objects.", "Information");
                                var hasInvalidObjects = false;

                                foreach (var databaseObject in newObjectList)
                                {
                                    if (databaseObject.TypeDesc == "SQL_STORED_PROCEDURE" ||
                                        databaseObject.TypeDesc == "SQL_TRIGGER" ||
                                        databaseObject.TypeDesc == "SQL_SCALAR_FUNCTION" ||
                                        databaseObject.TypeDesc == "SQL_TABLE_VALUED_FUNCTION" ||
                                        //databaseObject.TypeDesc == "SQL_INLINE_TABLE_VALUED_FUNCTION" ||
                                        databaseObject.TypeDesc == "VIEW")
                                    {
                                        var result = Objects.CheckDatabaseObject(databaseName, databaseObject);

                                        foreach (var error in result)
                                        {
                                            hasInvalidObjects = true;
                                            r.Add(new VerificationMessage()
                                                {
                                                    Message =
                                                        "While testing against " + dotNetNukeVersionInfo.Name +
                                                        " It appears the extension has created a database object: " +
                                                        databaseObject.Name + " with the following error: " +
                                                        Common.CleanError(error),
                                                    MessageType = MessageTypes.Error,
                                                    MessageId = new Guid("297ccdd3-884f-4cbb-85f0-24c343a9ff2a"),
                                                    Rule = GetType().ToString()
                                                });
                                        }
                                    }
                                }

                                //if DNN version > 6.0 and sq_refreshsqlmodule didn't return any errors, run SQL migration wizard
                                //Trace.WriteLine("SQL Scanner - about to run the SQL migration wizard.", "Information");
                                if (!hasInvalidObjects &&
                                    DotNetNukeVersions.ConvertNameToRank(dotNetNukeVersionInfo.Name) >= 60000 &&
                                    newObjectList.Count > 0)
                                {
                                    //Trace.WriteLine("SQL Scanner - This extension is new enough and as passed all scans up till now so it's time to run the migration wizard.", "Information");
                                    var hasAzureError = false;
                                    var databaseScanner = new DatabaseScanner();
                                    var output =
                                        databaseScanner.GenerateScriptFromSourceServer(
                                            Common.BuildServerConnection(databaseName), newObjectList);

                                    foreach (var error in output)
                                    {
                                        hasAzureError = true;
                                        r.Add(new VerificationMessage()
                                            {
                                                Message =
                                                    "While testing against " + dotNetNukeVersionInfo.Name +
                                                    " It appears the extension has an Azure SQL compatibility issue: " +
                                                    Common.CleanError(error),
                                                MessageType = MessageTypes.Error,
                                                MessageId = new Guid("98e44676-fde3-419b-bbf8-bcf785cbbf77"),
                                                Rule = GetType().ToString()
                                            });
                                    }

                                    if (!hasAzureError)
                                    {
                                        highestAzureCompatableVersion = dotNetNukeVersionInfo;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //Trace.WriteLine("SQL Scanner - There was a problem with the migration wizard.", "Information");

                                var error = ex.Message;
                                r.Add(new VerificationMessage()
                                    {
                                        Message =
                                            "While testing against " + dotNetNukeVersionInfo.Name +
                                            " the following error was returned: " + Common.CleanError(error),
                                        MessageType = MessageTypes.SystemError,
                                        MessageId = new Guid("2b0143ec-1bb6-43b1-9bae-1b114ec24e41"),
                                        Rule = GetType().ToString()
                                    });
                            }
                            finally
                            {
                                //drop database
                                Database.DropDatabase(databaseName);

                                User.DropDatabaseAdmin(databaseName);
                            }
                        }
                    }


                    //Trace.WriteLine("SQL Scanner - should we run the Azure install/backup/restore process?", "Information");

                    if (highestAzureCompatableVersion != null)
                    {

                        //Trace.WriteLine("SQL Scanner - Yes, we should!", "Information");
                        Stopwatch sw1 = Stopwatch.StartNew();
                        //if SQL migration wizard passes, do the following:
                        //      clear the azure install database 
                        try
                        {

                            //Trace.WriteLine("SQL Scanner - clear the Azure database.", "Information");
                            Azure.ClearInstallAndRestoreDatabases();
                        }
                        catch (Exception ex)
                        {
                            var error = ex.InnerException.Message;
                            r.Add(new VerificationMessage()
                                {
                                    Message =
                                        "While testing against SQL Azure database version " +
                                        highestAzureCompatableVersion.Name + " the cleanup failed and returned an error: " +
                                        Common.CleanError(error),
                                    MessageType = MessageTypes.SystemError,
                                    MessageId = new Guid("fdde5e65-8a39-41c5-8a9e-9d208a8df609"),
                                    Rule = GetType().ToString()
                                });
                        }

                        sw1.Stop();

                        r.Add(new VerificationMessage()
                            {
                                Message =
                                    "Processed Azure initial clean up in " + (sw1.ElapsedMilliseconds/1000) + " seconds",
                                MessageType = MessageTypes.Info,
                                MessageId = new Guid("1634b15a-a18b-422c-97f4-12f058c5e03b"),
                                Rule = GetType().ToString()
                            });

                        Stopwatch sw2 = Stopwatch.StartNew();

                        //install the base version scripts
                        try
                        {

                            //Trace.WriteLine("SQL Scanner - set up the base DNN tables in Azure.", "Information");
                            Azure.SetupBaseDNNDatabase(highestAzureCompatableVersion);
                        }
                        catch (Exception ex)
                        {
                            var error = ex.InnerException.Message;
                            r.Add(new VerificationMessage()
                                {
                                    Message =
                                        "While testing against SQL Azure database version " +
                                        highestAzureCompatableVersion.Name +
                                        " the base DNN table setup failed and returned an error: " +
                                        Common.CleanError(error),
                                    MessageType = MessageTypes.SystemError,
                                    MessageId = new Guid("898526c6-c8df-4d64-bb35-a04e5bc01044"),
                                    Rule = GetType().ToString()
                                });
                        }

                        sw2.Stop();

                        r.Add(new VerificationMessage()
                            {
                                Message = "Processed Azure initial setup in " + (sw2.ElapsedMilliseconds/1000) + " seconds",
                                MessageType = MessageTypes.Info,
                                MessageId = new Guid("f7eeb9b8-6a09-453f-814f-a6a5c126c431"),
                                Rule = GetType().ToString()
                            });

                        Stopwatch sw3 = Stopwatch.StartNew();

                        //install extension scripts
                        foreach (var sqlScriptFile in manifest.InstallScripts)
                        {
                            if (!File.Exists(sqlScriptFile.TempFilePath) || sqlScriptFile.IsUnInstall) continue;

                            try
                            {
                                //Trace.WriteLine("SQL Scanner - Install the extensions script files in Azure.", "Information");
                                Azure.InstallScripts(File.ReadAllText(sqlScriptFile.TempFilePath));
                            }
                            catch (Exception ex)
                            {
                                var error = ex.InnerException.Message;
                                r.Add(new VerificationMessage()
                                    {
                                        Message =
                                            "While testing against SQL Azure database version " +
                                            highestAzureCompatableVersion.Name + " " + sqlScriptFile.Name +
                                            " returned an error: " + Common.CleanError(error),
                                        MessageType = MessageTypes.Error,
                                        MessageId = new Guid("84d55196-a64e-4dbe-b848-2c06ba016869"),
                                        Rule = GetType().ToString()
                                    });
                            }
                        }

                        sw3.Stop();

                        r.Add(new VerificationMessage()
                            {
                                Message = "Processed Azure module setup in " + (sw3.ElapsedMilliseconds/1000) + " seconds",
                                MessageType = MessageTypes.Info,
                                MessageId = new Guid("845b161f-b3c4-4382-8cf5-c105918a891b"),
                                Rule = GetType().ToString()
                            });

                        Stopwatch sw4 = Stopwatch.StartNew();

                        //back up database
                        try
                        {

                            //Trace.WriteLine("SQL Scanner - Create a backup of the Azure database.", "Information");
                            Azure.CreateDatabaseBackup();
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("Error while backing up the azure database.");
                            Trace.TraceError(ex.Message);

                            if (ex.InnerException != null)
                            {
                                Trace.TraceError(ex.InnerException.Message);
                            }

                            var error = ex.InnerException.Message;
                            r.Add(new VerificationMessage()
                                {
                                    Message =
                                        "While testing against SQL Azure database version " +
                                        highestAzureCompatableVersion.Name +
                                        " an attempt to backup the database resulted in the following error: " +
                                        Common.CleanError(error),
                                    MessageType = MessageTypes.Error,
                                    MessageId = new Guid("03d76ba9-e2cc-4531-a322-3147d7cd1358"),
                                    Rule = GetType().ToString()
                                });
                        }

                        sw4.Stop();

                        r.Add(new VerificationMessage()
                            {
                                Message = "Processed Azure back up in " + (sw4.ElapsedMilliseconds/1000) + " seconds",
                                MessageType = MessageTypes.Info,
                                MessageId = new Guid("382d9b43-a8d0-4a69-8afe-f5f48a82d7dd"),
                                Rule = GetType().ToString()
                            });

                        Stopwatch sw5 = Stopwatch.StartNew();

                        //restore database
                        try
                        {

                            //Trace.WriteLine("SQL Scanner - Restore the Azure database.", "Information");
                            Azure.RestoreDatabaseBackup();
                        }
                        catch (Exception ex)
                        {
                            var error = ex.InnerException.Message;
                            r.Add(new VerificationMessage()
                                {
                                    Message =
                                        "While testing against SQL Azure database version " +
                                        highestAzureCompatableVersion.Name +
                                        " an attempt to restore the database resulted in the following error: " +
                                        Common.CleanError(error),
                                    MessageType = MessageTypes.Error,
                                    MessageId = new Guid("50274d0e-bc59-4b50-8e28-d35183a29428"),
                                    Rule = GetType().ToString()
                                });
                        }

                        sw5.Stop();

                        r.Add(new VerificationMessage()
                            {
                                Message = "Processed Azure restore in " + (sw5.ElapsedMilliseconds/1000) + " seconds",
                                MessageType = MessageTypes.Info,
                                MessageId = new Guid("6bbbe53d-4537-4c09-b1b5-c4b4b6fdbdf8"),
                                Rule = GetType().ToString()
                            });

                        Stopwatch sw6 = Stopwatch.StartNew();

                        //Clean Up
                        try
                        {
                            //Trace.WriteLine("SQL Scanner - perform the final clean up of the Azure databases and backups.", "Information");
                            Azure.ClearInstallAndRestoreDatabases();
                        }
                        catch (Exception ex)
                        {
                            var error = ex.InnerException.Message;
                            r.Add(new VerificationMessage()
                                {
                                    Message =
                                        "While testing against SQL Azure database version " +
                                        highestAzureCompatableVersion.Name + " the cleanup failed and returned an error: " +
                                        Common.CleanError(error),
                                    MessageType = MessageTypes.SystemError,
                                    MessageId = new Guid("fdde5e65-8a39-41c5-8a9e-9d208a8df609"),
                                    Rule = GetType().ToString()
                                });
                        }

                        sw6.Stop();

                        r.Add(new VerificationMessage()
                            {
                                Message = "Processed Azure final clean up in " + (sw6.ElapsedMilliseconds/1000) + " seconds",
                                MessageType = MessageTypes.Info,
                                MessageId = new Guid("e7da9c33-3ef4-430e-b8be-44188f09d5c7"),
                                Rule = GetType().ToString()
                            });
                    }

                    sw.Stop();

                    r.Add(new VerificationMessage()
                        {
                            Message = "Processed SQL in " + (sw.ElapsedMilliseconds/1000) + " seconds",
                            MessageType = MessageTypes.Info,
                            MessageId = new Guid("ad338269-f31c-4fe2-b564-4e5b44ac1b6b"),
                            Rule = GetType().ToString()
                        });
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    var error = ex.InnerException.Message;
                    r.Add(new VerificationMessage()
                    {
                        Message = "While testing the SQL scripts, the following error was returned: " + Common.CleanError(error),
                        MessageType = MessageTypes.SystemError,
                        MessageId = new Guid("97b6a0e6-1508-438e-a988-319e531c411c"),
                        Rule = GetType().ToString()
                    });
                }
                else
                {
                    var error = ex.Message;
                    r.Add(new VerificationMessage()
                    {
                        Message = "While testing the SQL scripts, the following error was returned: " + Common.CleanError(error),
                        MessageType = MessageTypes.SystemError,
                        MessageId = new Guid("5b2ee4ba-ebc0-4230-8504-aa3365acfb0c"),
                        Rule = GetType().ToString()
                    });
                }
            }
            
            return r;
        }
    }
}