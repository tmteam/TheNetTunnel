namespace TNT.Exceptions
{
    public class RemoteContractImplementationException : RemoteCallException
    {

        public RemoteContractImplementationException(int cordId,  string message = null) : base(true, RemoteCallExceptionId.RemoteContractImplementationException, message)
        {
            CordId = cordId;
        }

        public int CordId { get; }
    }
}