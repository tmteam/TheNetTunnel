using System;
using TNT.Exceptions;
using TNT.Exceptions.Remote;

namespace TNT.Cord
{
    public class ExceptionMessage
    {
        public ExceptionMessage(RemoteExceptionBase exception)
        {
            Exception = exception;
            ExceptionType = exception.Id;
            CordId = exception.CordId ?? 0;
            AskId = exception.AskId ?? 0;
            AdditionalExceptionInformation = exception.Message;
        }

        public ExceptionMessage(short cordId, short askId, RemoteExceptionId type, string additionalExceptionInformation)
        {
            this.CordId = cordId;
            this.AskId = askId;
            ExceptionType = type;
            Exception = RemoteExceptionBase.Create(type, additionalExceptionInformation, cordId, askId);
        }
        public static ExceptionMessage CreateBy(short? cordId, short? askId, Exception exception)
        {
            var rcException = (exception as RemoteExceptionBase)
                ??new RemoteUnhandledException(cordId, askId, exception, exception.ToString());
            return CreateBy(rcException);
        }
        public static ExceptionMessage CreateBy(RemoteExceptionBase rcExccException)
        {
            return  new ExceptionMessage(rcExccException);
        }
        public short CordId { get; set; }
        public short AskId { get; set; }
        public RemoteExceptionId ExceptionType { get; set; }
        public string AdditionalExceptionInformation { get; set; }
        public  RemoteExceptionBase Exception { get; }
    }
}