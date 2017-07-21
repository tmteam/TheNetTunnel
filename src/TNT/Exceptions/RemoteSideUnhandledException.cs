namespace TNT.Exceptions
{
    public class RemoteSideUnhandledException : RemoteCallException
    {
        public RemoteSideUnhandledException(int? cordId, int? askId, string message = null ) 
            :base(false, RemoteCallExceptionId.RemoteSideUnhandledException, message)
        {
        }
    }
}