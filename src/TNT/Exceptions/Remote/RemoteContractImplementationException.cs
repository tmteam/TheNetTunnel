using TNT.Exceptions.Remote;

namespace TNT.Exceptions.ContractImplementation
{
    public class RemoteContractImplementationException : RemoteExceptionBase
    {
        public RemoteContractImplementationException(short cordId, short? askId, bool isFatal,   string message = null) 
            : base(RemoteExceptionId.RemoteContractImplementationException, message)
        {
            CordId = cordId;
            AskId = askId;
            IsFatal = isFatal;
        }

    }
}