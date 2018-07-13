using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackageVerification.Models
{
    public class VerificationMessage
    {
        public string Message { get; set; }
        public MessageTypes MessageType { get; set; }
        public string Rule { get; set; }
        public Guid MessageId { get; set; }

        public override string ToString()
        {
            return string.Format("({1} in {2}) - {0}", Message, MessageType.ToString(), Rule);
        }
    }
}
