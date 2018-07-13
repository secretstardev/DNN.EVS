using EVSAppController.Controllers;

namespace EVSAppController.Models
{
    public class ExtensionMessage
    {
        private string _messageType;
        private string _messageLink = "";
        public int ExtensionMessageID { get; set; }
        public int ExtensionID { get; set; }
        public int MessageTypeID { get; set; }
        public string MessageID { get; set; }
        public string Message { get; set; }
        public string Rule { get; set; }

        [PetaPoco.Ignore]
        public string MessageType
        {
            get { return _messageType ?? (_messageType = MessageTypeController.GetMessageTypeByID(MessageTypeID).Type); }
        }

        [PetaPoco.Ignore]
        public string MessageLink
        {
            get
            {
                if (_messageLink != "")
                {
                    return _messageLink;
                }

                var messageLinkObject = MessageLinkController.GetMessageLink(MessageID);

                _messageLink = messageLinkObject != null ? messageLinkObject.Link : "";

                return _messageLink;
            }
        }
    }
}