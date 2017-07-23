namespace TNT.Exceptions.Remote
{
    public class RemoteSerializationException : RemoteExceptionBase
    {
        public RemoteSerializationException(
            short? messageId, 
            short? askId = null, 
            bool isFatal = true, 
            string message = null) 
            : base(RemoteExceptionId.RemoteSideSerializationException, message)
        {
            IsFatal = isFatal;
            MessageId = messageId;
            AskId = askId;
        }
    }
}