namespace TNT.Exceptions.Remote;

public class RemoteSerializationException : RemoteException
{
    public RemoteSerializationException(
        short? messageId, 
        short? askId = null, 
        bool isFatal = true, 
        string message = null) 
        : base(ErrorType.SerializationError, isFatal, messageId, askId, message)
    {
    }
}