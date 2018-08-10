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

namespace PackageVerification.SQLRunner
{
    public class Database
    {
        public static void CreateDatabase(string databaseName)
        {
            var database = new Models.Database { DatabaseName = databaseName };
            Common.RunSQLStatement("master", database.CreateDBQueryShort());

            var schemaAndRole = String.Format(@"USE [{0}];
                GO
                CREATE SCHEMA [TestSchema] AUTHORIZATION [dbo];
                GO
                CREATE ROLE [TestRole];
                GO
                ALTER AUTHORIZATION ON SCHEMA::[TestSchema] TO [TestRole];
                GO
                GRANT CREATE TABLE TO [TestRole];
                GO
                GRANT CREATE VIEW TO [TestRole];
                GO
                GRANT CREATE PROCEDURE TO [TestRole];
                GO
                GRANT CREATE FUNCTION TO [TestRole];
                GO", databaseName);

            Common.RunSQLScript(databaseName, schemaAndRole);
        }

        public static void DropDatabase(string databaseName)
        {
            var script = String.Format(@"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
                                        GO
                                        DROP DATABASE [{0}]
                                        GO", databaseName);

            Common.RunSQLScriptViaSMO("master", script);
        }
    }
}