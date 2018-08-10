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