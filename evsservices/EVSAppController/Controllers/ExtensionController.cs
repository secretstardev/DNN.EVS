using EVSAppController.Models;

namespace EVSAppController.Controllers
{
    public class ExtensionController : PetaPocoBaseClass
    {
        //create

        public static void AddExtension(Extension extension)
        {
            EVSDatabase.Insert("Extension", "ExtensionID", true, extension);
        }

        //read

        public static Extension GetExtension(int extensionId)
        {
            return EVSDatabase.First<Extension>("SELECT * FROM [dbo].[Extension] WHERE ExtensionID = @0", extensionId);
        }

        public static Extension GetExtension(string fileId)
        {
            return EVSDatabase.First<Extension>("SELECT * FROM [dbo].[Extension] WHERE FileID LIKE @0", fileId);
        }

        //update

        //delete

        //business logic
    }
}