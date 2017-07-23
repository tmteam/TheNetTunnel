using System;
using TNT.Exceptions.Remote;

namespace TNT.Presentation
{
    public class ExceptionMessage
    {
        public ExceptionMessage(RemoteExceptionBase exception)
        {
            Exception = exception;
            ExceptionType = exception.Id;
            messageId = exception.MessageId ?? 0;
            AskId = exception.AskId ?? 0;
            AdditionalExceptionInformation = exception.Message;
        }

        public ExceptionMessage(short messageId, short askId, RemoteExceptionId type, string additionalExceptionInformation)
        {
            this.messageId = messageId;
            this.AskId = askId;
            ExceptionType = type;
            Exception = RemoteExceptionBase.Create(type, additionalExceptionInformation, messageId, askId);
        }
        public static ExceptionMessage CreateBy(short? messageId, short? askId, Exception exception)
        {
            var rcException = (exception as RemoteExceptionBase)
                ??new RemoteUnhandledException(messageId, askId, exception, exception.ToString());
            return CreateBy(rcException);
        }
        public static ExceptionMessage CreateBy(RemoteExceptionBase rcExccException)
        {
            return  new ExceptionMessage(rcExccException);
        }
        public short messageId { get; set; }
        public short AskId { get; set; }
        public RemoteExceptionId ExceptionType { get; set; }
        public string AdditionalExceptionInformation { get; set; }
        public  RemoteExceptionBase Exception { get; }
    }
}