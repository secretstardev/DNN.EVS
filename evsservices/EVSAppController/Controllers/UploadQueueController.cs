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
using System.Linq;
using EVSAppController.Models;

namespace EVSAppController.Controllers
{
    public class UploadQueueController : PetaPocoBaseClass
    {
        //create

        public static void AddItemToQueue(UploadQueue queue)
        {
            EVSDatabase.Insert("UploadQueue", "UploadQueueID", true, queue);

            UpdateItemQueueStatus();
        }

        //read
        
        private static UploadQueue GetNextItemFromQueue()
        {
            try
            {
                return EVSDatabase.First<UploadQueue>("SELECT TOP 1 * FROM [dbo].[UploadQueue] WHERE [Processing] = 0 ORDER BY [UploadedDate] ASC");
            }
            catch //(Exception exc)
            {
                return null;
            }
        }

        public static List<UploadQueue> GetUploadQueueList()
        {
            return EVSDatabase.Fetch<UploadQueue>("SELECT * FROM [dbo].[UploadQueue]");
        }

        //update

        private static void LockItem(int uploadQueueId)
        {
            EVSDatabase.Execute("UPDATE [dbo].[UploadQueue] SET [Processing] = 1 WHERE [UploadQueueID] = @0", uploadQueueId);
        }

        //delete

        public static void RemoveItemFromQueue(UploadQueue queue)
        {
            EVSDatabase.Delete("UploadQueue", "UploadQueueID", queue);

            UpdateItemQueueStatus();
        }

        //business logic

        public static UploadQueue GetAndLockItemForProcessing()
        {
            var item = GetNextItemFromQueue();

            if (item != null)
            {
                LockItem(item.UploadQueueID);
            }

            return item;
        }

        public static void UpdateItemQueueStatus()
        {
            var queue = GetUploadQueueList();
            var processingItems = queue.Count(uploadQueue => uploadQueue.Processing);
            var totalItems = queue.Count;
            var itemCounter = 0;

            foreach (var uploadQueue in queue)
            {
                itemCounter++;
                var otheritems = (totalItems - processingItems);
                
                if (otheritems <= 0)
                    otheritems = 1;

                var currentProgress = ((itemCounter) / otheritems) * 100;
                var status = new Status
                    {
                        FileID = uploadQueue.FileID,
                        UserID = uploadQueue.UserKey,
                        Finished = false,
                        CurrentProgress = currentProgress
                    };

                if (currentProgress == 100 && !uploadQueue.Processing)
                {
                    status.CurrentMessage = "Your extension is next to begin processing.";
                    status.OverAllProgress = 32;
                }
                else if (uploadQueue.Processing)
                {
                    status.CurrentMessage = "Your extension has started processing.";
                    status.OverAllProgress = 35;
                }
                else
                {
                    status.CurrentMessage = "Your extension is currently behind " + (itemCounter - 1) + " other extension waiting to be processed.";
                    status.OverAllProgress = 31;
                }

                StatusController.AddStatus(status);
            }
        }
    }
}