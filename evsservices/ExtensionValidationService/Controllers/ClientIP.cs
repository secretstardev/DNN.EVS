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
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;

namespace ExtensionValidationService.Controllers
{
    public class ClientIP
    {
        public static string GetClientIp(HttpRequestBase request)
        {
            if(request!= null)
            {
                try
                {
                    var ipList = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                    if (!string.IsNullOrEmpty(ipList))
                    {
                        return ipList.Split(',')[0];
                    }

                    return request.ServerVariables["REMOTE_ADDR"];
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return "";
        }

        public static string GetClientIp(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                var httpContext = (HttpContextWrapper) request.Properties["MS_HttpContext"];
                var innerRequest = httpContext.Request;
                var clientIp = GetClientIp(innerRequest);
                return clientIp;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop;
                prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else
            {
                return null;
            }
        }
    }
}