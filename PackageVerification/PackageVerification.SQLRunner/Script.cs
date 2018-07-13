namespace PackageVerification.SQLRunner
{
    public class Script
    {
        public static void Execute(string databaseName, string script)
        {
            Common.RunSQLScriptViaSMO(databaseName, script, false, true);
        }
    }
}