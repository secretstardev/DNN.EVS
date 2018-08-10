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
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using EVSAppController;
using EVSAppController.Models;
using ExtensionValidationService.Formatters;
using Microsoft.WindowsAzure.ServiceRuntime;
using SendGridMail;
using SendGridMail.Transport;

namespace ExtensionValidationService.Controllers
{
    public class SendResultsController : ApiController
    {
        public HttpResponseMessage Post()
        {
            var nvc = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            var fileId = nvc["fileId"];
            var email = nvc["email"].ToLower();
            HttpResponseMessage resp;

            if (EmailIsValid(email))
            {
                resp = Request.CreateResponse(HttpStatusCode.OK);

                SendMail(fileId, email);
            }
            else
            {
                resp = Request.CreateResponse(HttpStatusCode.NotFound);
            }

            resp.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST");
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Headers", "x-requested-with");
            return resp;
        }

        private static void SendMail(string fileId, string email)
        {
            var extensionOutput = EVSAppController.Controllers.ExtensionController.GetExtension(fileId);

            var smtpserver = RoleEnvironment.GetConfigurationSettingValue(Constants.SmtpServer);
            var username = RoleEnvironment.GetConfigurationSettingValue(Constants.SmtpUsername);
            var password = RoleEnvironment.GetConfigurationSettingValue(Constants.SmtpPassword);
            var from = RoleEnvironment.GetConfigurationSettingValue(Constants.EmailFrom);
            var emailTo = RoleEnvironment.GetConfigurationSettingValue(Constants.EmailTo);

            //create a new message object
            var message = SendGrid.GetInstance();

            //set the message recipients
            message.AddTo(email);
            message.AddBcc(emailTo);

            //set the sender
            message.From = new MailAddress(from);

            //set the message body
            message.Html = BuildEmailBody(extensionOutput);

            //set the message subject
            message.Subject = "EVS: Review your verification results";

            //build attachments
            var filename = extensionOutput.OriginalFileName + "_EVS_Results";
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var xlsxFormatter = new ExtensionMessageXlsxFormatter();
            var xlsxFileLocation = tempPath + filename + ".xlsx";
            Stream xlsxFileStream = File.Create(xlsxFileLocation);
            xlsxFormatter.WriteToStream(extensionOutput.ExtensionMessages, xlsxFileStream);
            message.AddAttachment(xlsxFileLocation);

            var csvFormatter = new ExtensionMessageCsvFormatter();
            var csvFileLocation = tempPath + filename + ".csv";
            Stream csvFileStream = File.Create(csvFileLocation);
            csvFormatter.WriteToStream(extensionOutput.ExtensionMessages, csvFileStream);
            message.AddAttachment(csvFileLocation);

            var xmlFormatter = new ExtensionMessageXmlFormatter();
            var xmlFileLocation = tempPath + filename + ".xml";
            Stream xmlFileStream = File.Create(xmlFileLocation);
            xmlFormatter.WriteToStream(extensionOutput.ExtensionMessages, xmlFileStream);
            message.AddAttachment(xmlFileLocation);

            //create an instance of the SMTP transport mechanism
            var transportInstance = SMTP.GetInstance(new NetworkCredential(username, password), smtpserver);

            //send the mail
            transportInstance.Deliver(message);
        }

        private static string BuildEmailBody(Extension extension)
        {
            var emailBody = @"<!DOCTYPE html PUBLIC "" -//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">";
            emailBody += @"<html xmlns=""http://www.w3.org/1999/xhtml"">";
            emailBody += @"<head>";
            emailBody += @"<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />";
            emailBody += @"<title>EVS Email Notification</title>";
            emailBody += @"</head>";
            emailBody += @"<body>";
            emailBody += @"<table style=""font-family: Arial, Helvetica, sans-serif; border: 1px solid #CCC;"" width=""650px"" cellpadding=""10"">";
            emailBody += @"<th><img src=""http://evs.dnnsoftware.com/Images/header.jpg"" alt=""header""/></th>";
            emailBody += @"<tr><td>Thank-you for using the new <a href=""http://evs.dnnsoftware.com""> Extension Verification Service</a></td></tr>";
            emailBody += @"<tr><td>The extension selected has completed processing.</td></tr>";
            emailBody += @"<tr><td><strong>Extension Selected:</strong>&nbsp;" + extension.OriginalFileName + @"</td></tr>";
            
            foreach (var item in extension.ExtensionDetails)
            {
                emailBody += @"<tr><td><strong>" + item.DetailName + @":</strong>&nbsp;" + item.DetailValue + "</td></tr>";
            }
            
            emailBody += @"<tr><td>A file has been attached to this email containing the full verification results.</td></tr>";
            emailBody += @"<tr><td>If you have any questions or feedback, please visit the <a href=""http://bit.ly/evsfeedback""> Extension Verification Service forum</a> on<a href=""http://www.dotnetnuke.com/""> DotNetNuke.com.</a></td></tr>";
            emailBody += @"<table>";
            emailBody += @"</body>";
            emailBody += @"</html>";
            return emailBody;
        }

        private static bool EmailIsValid(string email)
        {
            const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                                   + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                                   + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.IsMatch(email);
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