namespace TNT.Exceptions.Remote
{
    public class RemoteContractImplementationException : RemoteExceptionBase
    {
        public RemoteContractImplementationException(short messageId, short? askId, bool isFatal,   string message = null) 
            : base(RemoteExceptionId.RemoteContractImplementationException, message)
        {
            MessageId = messageId;
            AskId = askId;
            IsFatal = isFatal;
        }

    }
}