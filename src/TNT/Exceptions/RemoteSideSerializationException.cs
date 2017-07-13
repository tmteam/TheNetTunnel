namespace TNT.Exceptions
{
    public class RemoteSideSerializationException : RemoteCallException
    {
        public RemoteSideSerializationException(string message = null) : base(true, RemoteCallExceptionId.RemoteSideSerializationException, message)
        {
        }
    }
}