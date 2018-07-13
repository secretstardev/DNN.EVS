using System;

namespace EVSAppController.Models
{
    public class UploadQueue
    {
        public int UploadQueueID { get; set; }
        public string FileID { get; set; }
        public string OriginalFileName { get; set; }
        public string FileLocation { get; set; }
        public string ContentType { get; set; }
        public long ContentSize { get; set; }
        public string UserKey { get; set; }
        public string UserIP { get; set; }
        public DateTime UploadedDate { get; set; }
        public bool Processing { get; set; }
    }
}