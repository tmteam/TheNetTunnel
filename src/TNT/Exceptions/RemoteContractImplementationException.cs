namespace TNT.Exceptions
{
    public class RemoteContractImplementationException : RemoteCallException
    {
        private readonly int _cordId;

        public RemoteContractImplementationException(int cordId,  string message = null) : base(true, RemoteCallExceptionId.RemoteContractImplementationException, message)
        {
            _cordId = cordId;
        }

        public int CordId
        {
            get { return _cordId; }
        }
    }
}