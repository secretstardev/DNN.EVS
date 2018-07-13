
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