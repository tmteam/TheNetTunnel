namespace TNT.Exceptions
{
    public class RemoteSideUnhandledException : RemoteCallException
    {
        public RemoteSideUnhandledException(string message = null) :base(false, RemoteCallExceptionId.RemoteSideUnhandledException, message)
        {
        }
    }
}