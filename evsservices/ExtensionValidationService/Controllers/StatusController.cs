using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using EVSAppController.Models;

namespace ExtensionValidationService.Controllers
{
    public class StatusController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var nvc = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            var userId = nvc["userId"];
            var fileId = nvc["fileId"];

            var resp = Request.CreateResponse(HttpStatusCode.OK);

            var statusOutput = EVSAppController.Controllers.StatusController.GetCurrentStatus(fileId, userId);

            resp.Content = new ObjectContent(typeof(Status), statusOutput, new JsonMediaTypeFormatter());
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