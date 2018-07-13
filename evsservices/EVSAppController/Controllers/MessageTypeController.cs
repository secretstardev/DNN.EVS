using System.Collections.Generic;
using EVSAppController.Models;

namespace EVSAppController.Controllers
{
    public class MessageTypeController : PetaPocoBaseClass
    {
        //create

        //read

        public static List<MessageType> GetMessageTypeList()
        {
            return EVSDatabase.Fetch<MessageType>("SELECT * FROM [dbo].[MessageType]");
        }

        public static MessageType GetMessageTypeByID(int messageTypeId)
        {
            return EVSDatabase.First<MessageType>("SELECT * FROM [dbo].[MessageType] WHERE [MessageTypeID] = @0", messageTypeId);
        }

        public static MessageType GetMessageTypeByType(string type)
        {
            return EVSDatabase.First<MessageType>("SELECT * FROM [dbo].[MessageType] WHERE [Type] LIKE @0", type);
        }

        //update

        //delete

        //business logic
    }
}