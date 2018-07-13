using System.Collections.Generic;
using EVSAppController.Models;

namespace EVSAppController.Controllers
{
    public class StatusController : PetaPocoBaseClass
    {
        //create

        public static void AddStatus(Status status)
        {
            EVSDatabase.Insert("Status", "StatusID", true, status);
        }

        //read

        public static Status GetStatus(int statusId)
        {
            return EVSDatabase.First<Status>("SELECT * FROM [dbo].[Status] WHERE StatusID = @0", statusId);
        }

        public static List<Status> GetStatuses(string fileId, string userId)
        {
            return EVSDatabase.Fetch<Status>("SELECT * FROM [dbo].[Status] WHERE [FileID] = @0 AND [UserID] = @1", fileId, userId);
        }

        public static Status GetCurrentStatus(string fileId, string userId)
        {
            return EVSDatabase.First<Status>("SELECT TOP 1 * FROM [dbo].[Status] WHERE [FileID] = @0 AND [UserID] = @1 ORDER BY StatusID DESC", fileId, userId);
        }

        //update

        //delete

        //business logic
    }
}