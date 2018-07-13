namespace EVSAppController.Models
{
    public class Status
    {
        public int StatusID { get; set; }
        public string FileID { get; set; }
        public string UserID { get; set; }
        public bool Finished { get; set; }
        public int OverAllProgress { get; set; }
        public int CurrentProgress { get; set; }
        public string CurrentMessage { get; set; }
    }
}