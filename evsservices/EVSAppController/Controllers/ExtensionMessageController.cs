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
using System.Collections.Generic;
using System.Diagnostics;
using EVSAppController.Models;

namespace EVSAppController.Controllers
{
    public class ExtensionMessageController : PetaPocoBaseClass
    {
        //create

        public static void AddExtensionMessage(ExtensionMessage extensionMessage)
        {
            try
            {
                EVSDatabase.Insert("ExtensionMessage", "ExtensionMessageID", true, extensionMessage);
            }
            catch //(Exception exc)
            {
                Trace.WriteLine("Could not add message to database", extensionMessage.Message);
            }
        }

        //read

        public static List<ExtensionMessage> GetExtensionMessageList(int extensionId)
        {
            return EVSDatabase.Fetch<ExtensionMessage>("SELECT * FROM [dbo].[ExtensionMessage] WHERE ExtensionID = @0", extensionId);
        }

        public static List<ExtensionMessage> GetExtensionMessageList(int extensionId, int messageTypeId)
        {
            return EVSDatabase.Fetch<ExtensionMessage>("SELECT * FROM [dbo].[ExtensionMessage] WHERE ExtensionID = @0 AND MessageTypeID = @1", extensionId, messageTypeId);
        }

        public static ExtensionMessage GetExtensionMessage(int extensionMessageID)
        {
            return EVSDatabase.First<ExtensionMessage>("SELECT * FROM [dbo].[ExtensionMessage] WHERE ExtensionMessageID = @0", extensionMessageID);
        }

        //update

        //delete

        //business logic
    }
}