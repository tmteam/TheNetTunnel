using System;
using TNT.Exceptions;

namespace TNT.Cord
{
    public class ExceptionMessage
    {
        public ExceptionMessage(RemoteCallException exception)
        {
            Exception = exception;
            ExceptionType = exception.Id;
            CordId = exception.CordId ?? 0;
            AskId = exception.AskId ?? 0;
            AdditionalExceptionInformation = exception.Message;
        }

        public ExceptionMessage(short cordId, short askId, RemoteCallExceptionId type, string additionalExceptionInformation)
        {
            this.CordId = cordId;
            this.AskId = askId;
            ExceptionType = type;
            Exception = RemoteCallException.Create(type, additionalExceptionInformation, cordId, askId);
        }
        public static ExceptionMessage CreateBy(short? cordId, short? askId, Exception exception)
        {
            var rcException = (exception as RemoteCallException)
                ??new RemoteSideUnhandledException(cordId, askId, exception.ToString());
            return CreateBy(rcException);
        }
        public static ExceptionMessage CreateBy(RemoteCallException rcExccException)
        {
            return  new ExceptionMessage(rcExccException);
        }
        public short CordId { get; set; }
        public short AskId { get; set; }
        public RemoteCallExceptionId ExceptionType { get; set; }
        public string AdditionalExceptionInformation { get; set; }
        public  RemoteCallException Exception { get; }
    }
}