using EVSAppController.Controllers;
using EVSAppController.Models;

namespace EVSAppController
{
    public class Messages
    {
        public static void LogMessage(Extension extension, string message, int currentProgress, int overAllProgress, bool finished = false)
        {
            LogMessage(extension.FileID, extension.UserKey, message, currentProgress, overAllProgress, finished);
        }

        public static void LogMessage(UploadQueue uploadQueue, string message, int currentProgress, int overAllProgress, bool finished = false)
        {
            LogMessage(uploadQueue.FileID, uploadQueue.UserKey, message, currentProgress, overAllProgress, finished);
        }

        public static void LogMessage(string fileId, string userId, string message, int currentProgress, int overAllProgress, bool finished = false)
        {
            StatusController.AddStatus(new Status()
            {
                CurrentMessage = message,
                CurrentProgress = currentProgress,
                FileID = fileId,
                Finished = finished,
                OverAllProgress = overAllProgress,
                UserID = userId
            });
        }

        public static void LogMessage(Extension extension, MessageList message)
        {
            LogMessage(extension.FileID, extension.UserKey, message);
        }

        public static void LogMessage(UploadQueue uploadQueue, MessageList message)
        {
            LogMessage(uploadQueue.FileID, uploadQueue.UserKey, message);
        }

        public static void LogMessage(string fileId, string userId, MessageList message)
        {
            switch (message)
            {
                    case MessageList.Start:
                    LogMessage(fileId, userId, "Your extension has been picked up by the processor.", 10, 40);
                    break;
                    case MessageList.LocalStorage:
                    LogMessage(fileId, userId, "Your extension has been moved to local storage.", 20, 45);
                    break;
                    case MessageList.UnZipped:
                    LogMessage(fileId, userId, "Your extension has been unzipped.", 30, 50);
                    break;
                    case MessageList.Processing:
                    LogMessage(fileId, userId, "Your extension is currently processing.", 10, 55);
                    break;
                    case MessageList.TwoWayFileChecker:
                    LogMessage(fileId, userId, "Running two way file checker.", 30, 60);
                    break;
                    case MessageList.AssemblyChecker:
                    LogMessage(fileId, userId, "Running assembly checker.", 60, 70);
                    break;
                    case MessageList.SQLChecker:
                    LogMessage(fileId, userId, "Running SQL checker (this could take a few minutes).", 70, 80);
                    break;
                    case MessageList.BuildingOutput:
                    LogMessage(fileId, userId, "Building output.", 80, 90);
                    break;
                    case MessageList.Finish:
                    LogMessage(fileId, userId, "Finish", 100, 100, true);
                    break;
            }
        }
    }

    public enum MessageList
    {
        Start,
        LocalStorage,
        UnZipped,
        Processing,
        TwoWayFileChecker,
        AssemblyChecker,
        SQLChecker,
        BuildingOutput,
        Finish
    }
}
