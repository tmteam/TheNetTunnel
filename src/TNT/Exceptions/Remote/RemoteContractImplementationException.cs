namespace TNT.Exceptions.Remote
{
    public class RemoteContractImplementationException : RemoteException
    {
        public RemoteContractImplementationException(short? messageId, short? askId, bool isFatal,   string message = null) 
            : base(ErrorType.ContractSignatureError, isFatal, messageId, askId, message)
        {
        }

    }
}