using System.Web.Http;
using System.Web.Routing;
using ExtensionValidationService.App_Start;

namespace ExtensionValidationService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ConfigureApis.ConfigureFormatters(GlobalConfiguration.Configuration);
        }
    }
}