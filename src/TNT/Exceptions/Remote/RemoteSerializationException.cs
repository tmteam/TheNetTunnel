namespace TNT.Exceptions.Remote
{
    public class RemoteSerializationException : RemoteExceptionBase
    {
        public RemoteSerializationException(
            short? cordId, 
            short? askId = null, 
            bool isFatal = true, 
            string message = null) 
            : base(RemoteExceptionId.RemoteSideSerializationException, message)
        {
            IsFatal = isFatal;
            CordId = cordId;
            AskId = askId;
        }
    }
}