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