using System.Collections.Generic;
using EVSAppController.Models;

namespace EVSAppController.Controllers
{
    public class ExtensionDetailController : PetaPocoBaseClass
    {
        //create

        public static void AddExtensionDetail(ExtensionDetail extensionDetail)
        {
            EVSDatabase.Insert("ExtensionDetail", "ExtensionDetailID", true, extensionDetail);
        }

        //read

        public static List<ExtensionDetail> GetExtensionDetailList(int extensionId)
        {
            return EVSDatabase.Fetch<ExtensionDetail>("SELECT * FROM [dbo].[ExtensionDetail] WHERE ExtensionID = @0", extensionId);
        }

        public static ExtensionDetail GetExtensionDetail(int extensionDetailID)
        {
            return EVSDatabase.First<ExtensionDetail>("SELECT * FROM [dbo].[ExtensionDetail] WHERE ExtensionDetailID = @0", extensionDetailID);
        }

        //update

        //delete

        //business logic
    }
}