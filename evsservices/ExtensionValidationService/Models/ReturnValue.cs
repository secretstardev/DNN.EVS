using System;

namespace ExtensionValidationService.Models
{
    public class ReturnValue
    {
        public Boolean ErrorInOperation { get; set; }
        public Boolean IsLastBlock { get; set; }
        public String Message { get; set; }

        public ReturnValue()
        {
            Message = "";
            ErrorInOperation = false;
            IsLastBlock = false;
        }
    }
}