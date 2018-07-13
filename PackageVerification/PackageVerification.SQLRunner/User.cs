using System;

namespace PackageVerification.SQLRunner
{
    public class User
    {
        public static void CreateDatabaseAdmin(string databaseName)
        {
            var createUserSQL = @"CREATE LOGIN [" + databaseName + @"_admin] WITH PASSWORD = '" + System.Configuration.ConfigurationManager.AppSettings["TestDBPassword"] + @"'; 
                GO";

            var addUserToDatabaseSQL = @"Use [" + databaseName + @"];
                GO

                IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'" + databaseName + @"_admin')
                BEGIN
                    CREATE USER [" + databaseName + @"_admin] FOR LOGIN [" + databaseName + @"_admin] WITH DEFAULT_SCHEMA=[TestSchema]
                    EXEC sp_addrolemember N'TestRole', N'" + databaseName + @"_admin'
                END;
                GO";

            Common.RunSQLScript("master", createUserSQL);
            Common.RunSQLScript(databaseName, addUserToDatabaseSQL);
        }

        public static void DropDatabaseAdmin(string databaseName)
        {
            var dropUserSQL = @"DROP LOGIN [" + databaseName + @"_admin]; 
                GO";

            Common.RunSQLScript("master", dropUserSQL);
        }
    }
}
