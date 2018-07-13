using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using EVSAppController.Models;
using ExtensionValidationService.Formatters;

namespace ExtensionValidationService.Controllers
{
    public class DownloadResultsController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var nvc = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            var fileId = nvc["fileId"];
            var type = nvc["type"].ToLower();

            var resp = Request.CreateResponse(HttpStatusCode.OK);

            var extensionOutput = EVSAppController.Controllers.ExtensionController.GetExtension(fileId);
            var filename = extensionOutput.OriginalFileName + "_EVS_Results";

            if (type == "xml")
            {
                resp.Content = new ObjectContent(typeof(List<ExtensionMessage>), extensionOutput.ExtensionMessages, new ExtensionMessageXmlFormatter(filename + ".xml"));
            }
            else if (type == "csv")
            {
                resp.Content = new ObjectContent(typeof(List<ExtensionMessage>), extensionOutput.ExtensionMessages, new ExtensionMessageCsvFormatter(filename + ".csv"));
            }
            else if (type == "xlsx")
            {
                resp.Content = new ObjectContent(typeof(List<ExtensionMessage>), extensionOutput.ExtensionMessages, new ExtensionMessageXlsxFormatter(filename + ".xlsx"));
            }
            else
            {
                resp.Content = new ObjectContent(typeof(List<ExtensionMessage>), extensionOutput.ExtensionMessages, new JsonMediaTypeFormatter());
            }

            resp.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, GET");
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Headers", "x-requested-with");
            return resp;
        }

        public HttpResponseMessage Options()
        {
            var resp = Request.CreateResponse(HttpStatusCode.OK);
            resp.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, GET");
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Headers", "x-requested-with");
            return resp;
        }
    }
}