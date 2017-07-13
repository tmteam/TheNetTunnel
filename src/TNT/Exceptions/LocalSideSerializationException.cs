namespace TNT.Exceptions
{
    public class LocalSideSerializationException : RemoteCallException
    {
        public LocalSideSerializationException(string message = null) : base(true, RemoteCallExceptionId.LocalSideSerializationException, message)
        {
        }
    }
}