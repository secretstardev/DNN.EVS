using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace ExtensionValidationService.Controllers
{
    public class UserIdController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var resp = Request.CreateResponse(HttpStatusCode.OK);

            var userIdOutput = new UserIdOutput { UserId = Guid.NewGuid().ToString() };

            resp.Content = new ObjectContent(typeof(UserIdOutput), userIdOutput, new JsonMediaTypeFormatter());
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

        public class UserIdOutput
        {
            public string UserId { get; set; }
        }
    }
}