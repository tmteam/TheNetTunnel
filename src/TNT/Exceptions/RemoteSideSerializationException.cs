namespace TNT.Exceptions
{
    public class RemoteSideSerializationException : RemoteCallException
    {
        public RemoteSideSerializationException(short cordId, short? askId = null, string message = null) : base(true, RemoteCallExceptionId.RemoteSideSerializationException, message)
        {
        }
    }
}