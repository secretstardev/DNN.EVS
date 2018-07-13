using Microsoft.WindowsAzure.ServiceRuntime;
using PetaPoco.Providers;

namespace EVSAppController.Controllers
{
    public class PetaPocoBaseClass
    {
        public static PetaPoco.Database EVSDatabase = new PetaPoco.Database(RoleEnvironment.GetConfigurationSettingValue(Constants.DataBaseName), new SqlServerDatabaseProvider());
    }
}
