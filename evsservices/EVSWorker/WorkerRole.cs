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
using System.Net;
using System.Net.Mail;
using System.Threading;
using EVSAppController;
using EVSAppController.Controllers;
using EVSAppController.Models;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using PackageVerification.AssemblyScanner;
using PackageVerification.Models;
using PackageVerification.Rules;
using PackageVerification.Rules.Manifest;
using PackageVerification.Rules.Manifest.Components;
using SendGridMail;
using SendGridMail.Transport;

namespace EVSWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            //Trace.WriteLine("EVSWorker entry point called", "Information");

            while (true)
            {
                Thread.Sleep(5*1000);
                //Trace.WriteLine("Working", "Information");

                var itemToProcess = UploadQueueController.GetAndLockItemForProcessing();

                if (itemToProcess != null)
                {
                    Process(itemToProcess);
                }
                else
                {
                    CleanTemp();
                }
            }
        }

        private static void Process(UploadQueue itemToProcess)
        {
            var p = new Package();
            var error = false;

            try
            {
                //Trace.WriteLine("Found an item to process", "Information");
                Messages.LogMessage(itemToProcess, MessageList.Start);

                //move the file to local server storage
                var tempFile = FileHelpers.MoveBlobToTempStorage(itemToProcess.FileLocation);

                Messages.LogMessage(itemToProcess, MessageList.LocalStorage);

                //build a new 'package' object
                p.Path = tempFile;
                if (p.Messages == null) p.Messages = new List<VerificationMessage>();

                //unzip the package
                var fz = new FastZip();

                var shortpath = Environment.GetEnvironmentVariable("ShortRootPath") ?? Path.GetTempPath();

                var tempPath = Path.Combine(shortpath, Guid.NewGuid().ToString());

                //Trace.WriteLine("Package location: " + tempPath);

                Directory.CreateDirectory(tempPath);
                p.ExtractedPath = tempPath;
                
                try
                {
                    using (var stm = new FileStream(p.Path, FileMode.Open))
                    {
                        fz.ExtractZip(stm, tempPath, FastZip.Overwrite.Always, null, null, null, true, true);
                    }
                }
                catch (Exception exc)
                {
                    error = true;
                    Trace.TraceError("Error while extracting file.");
                    Trace.TraceError(exc.Message);

                    if (exc.InnerException != null)
                    {
                        Trace.TraceError(exc.InnerException.Message);
                    }

                    p.Messages.Add(new VerificationMessage
                        {
                            Message = "Problem encountered while attempting to extract uploaded file.",
                            MessageId = new Guid("9ac011fa-08dd-4e26-91ba-0af7d5daf482"),
                            MessageType = MessageTypes.SystemError,
                            Rule = "Extraction"
                        });

                    goto Cleanup;
                }

                Messages.LogMessage(itemToProcess, MessageList.UnZipped);

                Messages.LogMessage(itemToProcess, MessageList.Processing);

                p.Messages.AddRange(new PackageExists().ApplyRule(p));
                p.Messages.AddRange(new MinPackageSize().ApplyRule(p));
                p.Messages.AddRange(new ManifestFileExists().ApplyRule(p));
                p.Messages.AddRange(new ManifestIsXML().ApplyRule(p));

                foreach (var manifest in p.DNNManifests)
                {
                    p.Messages.AddRange(new CorrectTypeAndVersion().ApplyRule(p, manifest));

                    switch (manifest.ManifestType)
                    {
                        case ManifestTypes.Package:
                            p.Messages.AddRange(new PackageNodeAndChildren().ApplyRule(p, manifest));
                            p.Messages.AddRange(new PackageOwner().ApplyRule(p, manifest));
                            p.Messages.AddRange(new AssemblyNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new AuthenticationSystemNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new DashboardControlNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new CleanupNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new ConfigNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new FileNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new ModuleNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new SkinNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new SkinObjectNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new ContainerNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new CoreLanguageNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new ExtensionLanguageNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new ResourceFileNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new ScriptNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new WidgetNode().ApplyRule(p, manifest));
                            break;
                        case ManifestTypes.Module:
                            p.Messages.AddRange(new FolderNodeAndChildren().ApplyRule(p, manifest));
                            p.Messages.AddRange(new FileNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new ModuleNode().ApplyRule(p, manifest));
                            p.Messages.AddRange(new ProcessScripts().ApplyRule(p, manifest));
                            break;
                    }

                    Messages.LogMessage(itemToProcess, MessageList.TwoWayFileChecker);
                    //Trace.WriteLine("Running two-way file checker.", "Information");
                    p.Messages.AddRange(new TwoWayFileChecker().ApplyRule(p, manifest));

                    Messages.LogMessage(itemToProcess, MessageList.AssemblyChecker);
                    //Trace.WriteLine("Running assembly checker.", "Information");
                    const string assemblyInfoFile = @"collection.xml";
                    p.MinDotNetVersion = ProcessAssembly.GetMinDotNetFramework(p.Assemblies).VersionName;
                    p.MinDotNetNukeVersion = ProcessAssembly.GetMinDotNetNukeVersion(p.Assemblies, assemblyInfoFile).Name;

                    Messages.LogMessage(itemToProcess, MessageList.SQLChecker);
                    //Trace.WriteLine("Running SQL scanner.", "Information");
                    p.Messages.AddRange(new SQLTestRunner().ApplyRule(p, manifest));
                }

                p.Messages.AddRange(new FileEncodingChecker().ApplyRule(p));
                p.Messages.AddRange(new DependencyChecker().ApplyRule(p));
                
                Messages.LogMessage(itemToProcess, MessageList.BuildingOutput);

                //Let's process the SQL output if there is any.
                if (p.SQLOutputPath != null)
                {
                    var tempPathForZip = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempPathForZip);
                    var filename = itemToProcess.FileID + ".zip";
                    var destination = tempPathForZip + "\\" + filename;
                    var zipInputStream = File.Create(destination);
                    
                    using (zipInputStream)
                    {
                        fz.CreateZip(zipInputStream, p.SQLOutputPath, true, "", "");
                    }

                    var zipStream = File.Open(destination, FileMode.Open);

                    using (zipStream)
                    {
                        var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue(Constants.ConfigurationSectionKey));
                        var blobClient = storageAccount.CreateCloudBlobClient();
                        var container = blobClient.GetContainerReference(Constants.ContainerNameScripts);
                        container.CreateIfNotExists();
                        var file = container.GetBlockBlobReference(filename);
                        file.UploadFromStream(zipStream);
                    }

                    try
                    {
                        File.Delete(destination);
                    }
                    catch (Exception exc)
                    {
                        error = true;
                        Trace.TraceError("Error while deleting temp SQL output files.");
                        Trace.TraceError(exc.Message);

                        if (exc.InnerException != null)
                        {
                            Trace.TraceError(exc.InnerException.Message);
                        }
                    }
                }

                //clean up
                Cleanup:
                try
                {
                    File.Delete(tempFile); //remove the temp file from the server.
                    DeleteDirectory(p.ExtractedPath); //remove the extracted folder from the server.

                    if (p.SQLOutputPath != null)
                    {
                        DeleteDirectory(p.SQLOutputPath);
                    }
                }
                catch (Exception exc)
                {
                    error = true;
                    Trace.TraceError("Error while deleting temp files.");
                    Trace.TraceError(exc.Message);

                    if (exc.InnerException != null)
                    {
                        Trace.TraceError(exc.InnerException.Message);
                    }
                }
            }
            catch (Exception exc)
            {
                error = true;
                Trace.TraceError("Error while processing extension.");
                Trace.TraceError(exc.Message);
                Trace.TraceError(exc.StackTrace);

                if (exc.InnerException != null)
                {
                    Trace.TraceError(exc.InnerException.Message);
                }
            }
            finally
            {
                UploadQueueController.RemoveItemFromQueue(itemToProcess); //remove the item from the queue.

                if(itemToProcess.UserKey != "44eb5af6-2205-488e-9985-f7204c268820")
                {
                    FileHelpers.RemoveBlob(itemToProcess.FileLocation); //this will remove the file from blob storage.
                }                

                var extension = new Extension(itemToProcess);

                ExtensionController.AddExtension(extension);

                ProcessResults(p, extension.ExtensionID, error);

                //report to the status service that this file has finished processing.
                Messages.LogMessage(itemToProcess, MessageList.Finish);
            }
        }

        private static void CleanTemp()
        {
            try
            {
                var tempFolder = Path.GetTempPath();
                var files = Directory.GetFiles(tempFolder);
                var dirs = Directory.GetDirectories(tempFolder);

                foreach (var file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (var dir in dirs)
                {
                    if (!dir.Contains("azureBackup"))
                    {
                        DeleteDirectory(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error while processing extension.");
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
            }
            
        }

        private static void DeleteDirectory(string targetDir)
        {
            var files = Directory.GetFiles(targetDir);
            var dirs = Directory.GetDirectories(targetDir);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        private static void ProcessResults(Package p, int extensionId, bool error)
        {
            try
            {
                //var flaggedAzure = false;
                var azureCompatable = true;
                var hasError = false;
                var hasWarning = false;
                var hasInfo = false;

                foreach (var message in p.Messages)
                {
                    var messageTypeId = 0;

                    switch (message.MessageType)
                    {
                        case MessageTypes.Error:
                            messageTypeId = 1;
                            hasError = true;
                            break;
                        case MessageTypes.Warning:
                            messageTypeId = 2;
                            hasWarning = true;
                            break;
                        case MessageTypes.Info:
                            messageTypeId = 3;
                            hasInfo = true;
                            break;
                        case MessageTypes.SystemError:
                            messageTypeId = 4;
                            break;
                    }

                    switch (message.MessageId.ToString())
                    {
                        case "d40fe32f-ea79-49c3-802c-f7264fb69a2b":
                            ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                                {
                                    DetailID = "6546126f-a24f-4e16-95d0-77d45bf7406e",
                                    DetailName = "Package Name",
                                    DetailValue = message.Message,
                                    ExtensionID = extensionId
                                });
                            break;
                        case "feaf4309-3ef3-4278-b610-b1f78fe3c6a9":
                            ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                                {
                                    DetailID = "bfbe0973-f78e-4095-85e6-138a4578a106",
                                    DetailName = "Package Type",
                                    DetailValue = message.Message,
                                    ExtensionID = extensionId
                                });
                            break;
                        case "07e29bf3-5816-4e1c-a9e8-7a05ee02a0f0":
                            ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                                {
                                    DetailID = "902951df-2c57-418c-a969-cc4f7944714b",
                                    DetailName = "Package Version",
                                    DetailValue = message.Message,
                                    ExtensionID = extensionId
                                });
                            break;
                        case "52416eb4-1001-4208-a627-0b2ced7b7212":
                        case "cfb86d07-9cb4-42c5-9367-96a3e45c3054":
                        case "6f6ed7be-c182-4baf-a369-809b7d99549a":
                        case "fdde5e65-8a39-41c5-8a9e-9d208a8df609":
                        case "50274d0e-bc59-4b50-8e28-d35183a29428":
                        case "03d76ba9-e2cc-4531-a322-3147d7cd1358":
                        case "84d55196-a64e-4dbe-b848-2c06ba016869":
                        case "98e44676-fde3-419b-bbf8-bcf785cbbf77":
                        case "297ccdd3-884f-4cbb-85f0-24c343a9ff2a":
                        case "b25d95e3-06d0-4241-9729-96f85cfddcbf":
                            azureCompatable = false;

                            ExtensionMessageController.AddExtensionMessage(new ExtensionMessage
                                {
                                    ExtensionID = extensionId,
                                    Message = message.Message,
                                    MessageID = message.MessageId.ToString(),
                                    MessageTypeID = messageTypeId,
                                    Rule = message.Rule
                                });

                            break;
                        default:
                            ExtensionMessageController.AddExtensionMessage(new ExtensionMessage
                                {
                                    ExtensionID = extensionId,
                                    Message = message.Message,
                                    MessageID = message.MessageId.ToString(),
                                    MessageTypeID = messageTypeId,
                                    Rule = message.Rule
                                });
                            break;
                    }
                }

                if (p.DNNManifests.Count > 0)
                {
                    foreach (var manifest in p.DNNManifests)
                    {
                        if (!(manifest.ManifestVersion() >= 5.0)) continue;

                        if (manifest.HasAzureCompatibleTag == false && azureCompatable)
                        {
                            //add warning that they should include the azureCompatable tag.
                            ExtensionMessageController.AddExtensionMessage(new ExtensionMessage
                                {
                                    ExtensionID = extensionId,
                                    Message =
                                        "Your extension has passed the Azure compatibility scan, however your manifest is missing the <azureCompatible>true</azureCompatible> tag.",
                                    MessageID = "5844f748-171c-448f-9470-8218ce02386e",
                                    MessageTypeID = 2,
                                    Rule = "Check Azure Compatible Tag"
                                });

                            hasWarning = true;
                        }
                        else if (!manifest.TaggedAzureCompatible && azureCompatable)
                        {
                            //add warning that they should change the azureCompatable tag to true.
                            ExtensionMessageController.AddExtensionMessage(new ExtensionMessage
                                {
                                    ExtensionID = extensionId,
                                    Message =
                                        "Your extension has passed the Azure compatibility scan, however your manifest has the following tag <azureCompatible>false</azureCompatible>. You should change the value to true.",
                                    MessageID = "5b7c22f2-d87b-45ca-baf7-b4b51b37d77e",
                                    MessageTypeID = 2,
                                    Rule = "Check Azure Compatible Tag"
                                });

                            hasWarning = true;
                        }
                        else if (manifest.TaggedAzureCompatible && !azureCompatable)
                        {
                            //add error that despite the azureCompatable tag being set to true, the package is still not azure compatible
                            ExtensionMessageController.AddExtensionMessage(new ExtensionMessage
                                {
                                    ExtensionID = extensionId,
                                    Message =
                                        "The <azureCompatible>true</azureCompatible> tag has been found in your manifest, however this extension didn't pass the Azure compatibility scan.",
                                    MessageID = "bba5b1b6-f731-4f0c-a7b8-fe3165044e86",
                                    MessageTypeID = 1,
                                    Rule = "Check Azure Compatible Tag"
                                });

                            hasError = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(p.MinDotNetNukeVersion))
                    {
                        ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                            {
                                DetailID = "b1886f0e-9234-4c71-b10f-f083bca1afcf",
                                DetailName = "Min DotNetNuke Version",
                                DetailValue = p.MinDotNetNukeVersion,
                                ExtensionID = extensionId
                            });
                    }

                    if (!string.IsNullOrEmpty(p.MinDotNetVersion))
                    {
                        ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                            {
                                DetailID = "c9042f5e-7119-48e6-9fd5-2288031760bc",
                                DetailName = "Min .Net Version",
                                DetailValue = p.MinDotNetVersion,
                                ExtensionID = extensionId
                            });
                    }

                    ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                        {
                            DetailID = "c3d24742-bda2-469b-a572-bb92dfb4e2b6",
                            DetailName = "Package Has Errors",
                            DetailValue = hasError.ToString(),
                            ExtensionID = extensionId
                        });

                    ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                        {
                            DetailID = "1ce06198-be6b-4994-801a-e226c5cb80ec",
                            DetailName = "Package Has Warnings",
                            DetailValue = hasWarning.ToString(),
                            ExtensionID = extensionId
                        });

                    ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                        {
                            DetailID = "7f41a0e8-6760-4bbb-9f3b-f53daf21cfe0",
                            DetailName = "Package Has Info Items",
                            DetailValue = hasInfo.ToString(),
                            ExtensionID = extensionId
                        });

                    ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                        {
                            DetailID = "b783e88d-39d4-4b6e-b0b0-89cf66aeae9e",
                            DetailName = "SQL Azure Compatible",
                            DetailValue = azureCompatable.ToString(),
                            ExtensionID = extensionId
                        });
                }
            }
            catch (Exception exc)
            {
                error = true;

                Trace.TraceError("Error while processing results.", exc.Message);

                ExtensionMessageController.AddExtensionMessage(new ExtensionMessage
                    {
                        ExtensionID = extensionId,
                        Message = "There was an error processing your results.",
                        MessageID = "cfb86d07-9cb4-42c5-9367-96a3e45c3054",
                        MessageTypeID = 4,
                        Rule = ""
                    });
            }
            finally
            {
                if (error)
                {
                    ExtensionDetailController.AddExtensionDetail(new ExtensionDetail
                    {
                        DetailID = "b234c8a1-c819-4b81-8056-e7107d805e0d",
                        DetailName = "System Error",
                        DetailValue = "True",
                        ExtensionID = extensionId
                    });

                    SendErrorNotification(extensionId);
                }
            }
        }

        private static void SendErrorNotification(int extensionId)
        {
            try
            {
                var smtpserver = RoleEnvironment.GetConfigurationSettingValue(Constants.SmtpServer);
                var username = RoleEnvironment.GetConfigurationSettingValue(Constants.SmtpUsername);
                var password = RoleEnvironment.GetConfigurationSettingValue(Constants.SmtpPassword);
                var from = RoleEnvironment.GetConfigurationSettingValue(Constants.EmailFrom);
                var emailTo = RoleEnvironment.GetConfigurationSettingValue(Constants.EmailTo);
                var transportInstance = SMTP.GetInstance(new NetworkCredential(username, password), smtpserver);

                //create a new message object
                var message = SendGrid.GetInstance();

                //set the message recipients
                message.AddTo(emailTo);

                //set the sender
                message.From = new MailAddress(from);

                //set the message body
                message.Html = string.Format("<html><p>Hello Nathan,</p><p>While processing this ({0}) package, I encountered an error!</p>"
                    + "<p>Best regards, <br />EVS</p></html>", extensionId);

                //set the message subject
                message.Subject = "EVS has encountered an error.";

                //send the mail
                transportInstance.Deliver(message);
            }
            catch (Exception exc)
            {
                Trace.TraceError("Error while sending error notification.", exc.Message);
            }
        }
        
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;
            ConfigureDiagnosticMonitor();
            // For information on handling configuration changes 
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357. 

            var customTempLocalResourcePath = RoleEnvironment.GetLocalResource("CustomTempLocalStore").RootPath;

            Environment.SetEnvironmentVariable("TMP", customTempLocalResourcePath);
            Environment.SetEnvironmentVariable("TEMP", customTempLocalResourcePath);
            Environment.SetEnvironmentVariable("ShortRootPath", "C:\\Resources\\directory\\S\\");
            
            //Trace.WriteLine("Local Temp Storage Location: " + customTempLocalResourcePath, "Information");

            return base.OnStart();
        }
        
        public static void ConfigureDiagnosticMonitor()
        {
            //Trace.TraceInformation("Configuring diagnostic monitor...");

            //// TODO put these settings on the service configuration file
            //var transferPeriod = TimeSpan.FromMinutes(1);
            //var bufferQuotaInMB = 100;

            //// Add Windows Azure Trace Listener
            //Trace.Listeners.Add(new DiagnosticMonitorTraceListener());

            //// Enable Collection of Crash Dumps
            //CrashDumps.EnableCollection(true);

            //// Get the Default Initial Config
            //var config = DiagnosticMonitor.GetDefaultInitialConfiguration();

            //// Windows Azure Logs
            //config.Logs.ScheduledTransferPeriod = transferPeriod;
            //config.Logs.BufferQuotaInMB = bufferQuotaInMB;
            //config.Logs.ScheduledTransferLogLevelFilter = Microsoft.WindowsAzure.Diagnostics.LogLevel.Verbose;

            //// File-based logs
            //config.Directories.ScheduledTransferPeriod = transferPeriod;
            //config.Directories.BufferQuotaInMB = bufferQuotaInMB;

            //config.DiagnosticInfrastructureLogs.ScheduledTransferPeriod = transferPeriod;
            //config.DiagnosticInfrastructureLogs.BufferQuotaInMB = bufferQuotaInMB;
            //config.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = LogLevel.Warning;

            //// Windows Event logs
            //config.WindowsEventLog.DataSources.Add("Application!*");
            //config.WindowsEventLog.DataSources.Add("System!*");
            //config.WindowsEventLog.ScheduledTransferPeriod = transferPeriod;
            //config.WindowsEventLog.ScheduledTransferLogLevelFilter = LogLevel.Information;
            //config.WindowsEventLog.BufferQuotaInMB = bufferQuotaInMB;

            //// Performance Counters
            //var counters = new List<string> {
            //    @"\Processor(_Total)\% Processor Time",
            //    @"\Memory\Available MBytes",
            //    @"\ASP.NET Applications(__Total__)\Requests Total",
            //    @"\ASP.NET Applications(__Total__)\Requests/Sec",
            //    @"\ASP.NET\Requests Queued",
            //};

            //counters.ForEach(
            //    counter => config.PerformanceCounters.DataSources.Add(
            //        new PerformanceCounterConfiguration { CounterSpecifier = counter, SampleRate = TimeSpan.FromSeconds(60) }));
            //config.PerformanceCounters.ScheduledTransferPeriod = transferPeriod;
            //config.PerformanceCounters.BufferQuotaInMB = bufferQuotaInMB;

            //DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", config);
            //Trace.TraceInformation("Diagnostics Setup complete");
        }
    }
}
