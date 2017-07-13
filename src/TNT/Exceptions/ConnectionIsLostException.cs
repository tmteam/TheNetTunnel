namespace TNT.Exceptions
{
    public class ConnectionIsLostException : RemoteCallException
    {
        public ConnectionIsLostException(string message = null):base(true, RemoteCallExceptionId.ConnectionIsLostException, message)
        {
        }
    }
}