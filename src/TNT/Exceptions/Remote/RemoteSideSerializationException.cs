namespace TNT.Exceptions
{
    public class RemoteSideSerializationException : RemoteCallException
    {
        public RemoteSideSerializationException(int cordId, int? askId = null, string message = null) : base(true, RemoteCallExceptionId.RemoteSideSerializationException, message)
        {
        }
    }
}