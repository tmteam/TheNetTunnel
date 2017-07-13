namespace TNT.Exceptions
{
    public class RemoteContractImplementationException : RemoteCallException
    {
        public RemoteContractImplementationException(string message = null) : base(true, RemoteCallExceptionId.RemoteContractImplementationException, message)
        {
        }
    }
}