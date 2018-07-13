using EVSAppController.Models;

namespace EVSAppController.Controllers
{
    public class MessageLinkController : PetaPocoBaseClass
    {
        //create

        //read

        public static MessageLink GetMessageLink(string messageID)
        {
            return EVSDatabase.SingleOrDefault<MessageLink>("SELECT * FROM [dbo].[MessageLink] WHERE MessageID = @0", messageID);
        }

        //update

        //delete

        //business logic
    }
}
