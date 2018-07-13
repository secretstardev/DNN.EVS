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