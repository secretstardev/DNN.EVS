using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ExtensionValidationService.Models;

namespace ExtensionValidationService.Controllers
{
    public class UploadBlockController: ApiController
    {
        public async Task<HttpResponseMessage> Post()
        {
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var nvc = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            var userId = nvc["userId"];
            var fileId = nvc["fileId"];
            var blockId = 0;
            int.TryParse(nvc["blockId"], out blockId);

            var multipartStreamProvider = new AsyncUploadBlockHandler(userId, fileId, blockId);

            try
            {
                await Request.Content.ReadAsMultipartAsync(multipartStreamProvider);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
            
            var resp = Request.CreateResponse(HttpStatusCode.OK);
            resp.Content = new ObjectContent(typeof(ReturnValue), multipartStreamProvider.ReturnValue, new JsonMediaTypeFormatter());
            resp.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST");
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Headers", "x-requested-with");
            return resp;
        }

        public HttpResponseMessage Options()
        {
            var resp = Request.CreateResponse(HttpStatusCode.OK);
            resp.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST");
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Headers", "x-requested-with");
            return resp;
        }

        
    }
}