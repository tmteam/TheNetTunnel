using System;
using TNT.Exceptions.Remote;

namespace TNT.Presentation
{
    public class ErrorMessage
    {
        public ErrorMessage(short? messageId, short? askId, ErrorType type, string additionalExceptionInformation)
        {
            this.MessageId = messageId;
            this.AskId = askId;
            ErrorType = type;
            Exception = RemoteException.Create(type, additionalExceptionInformation, messageId, askId);
        }
       
        public short? MessageId { get; set; }
        public short? AskId { get; set; }
        public ErrorType ErrorType { get; set; }
        public string AdditionalExceptionInformation { get; set; }
        public  RemoteException Exception { get; }
    }
}