using System.Web.Http;
using ExtensionValidationService.Formatters;

namespace ExtensionValidationService.App_Start
{
    public class ConfigureApis
    {
        public static void ConfigureFormatters(HttpConfiguration config)
        {
            config.Formatters.Add(new ExtensionMessageCsvFormatter()); 
            config.Formatters.Add(new ExtensionMessageXlsxFormatter());
            config.Formatters.Add(new ExtensionMessageXmlFormatter());
        }
    }
}