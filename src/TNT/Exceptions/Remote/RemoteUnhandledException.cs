using System;

namespace TNT.Exceptions.Remote
{
    public class RemoteUnhandledException : RemoteExceptionBase
    {
        public RemoteUnhandledException(short? messageId, short? askId,Exception innerException,  string message = null ) 
            :base(RemoteExceptionId.RemoteSideUnhandledException, message, innerException)
        {
            IsFatal = false;
            MessageId = messageId;
            AskId = askId;
        }
    }
}