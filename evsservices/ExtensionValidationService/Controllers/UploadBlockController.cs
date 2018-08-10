#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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