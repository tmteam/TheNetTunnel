using System;

namespace TNT.Exceptions.Local;

public abstract class LocalException: Exception, ITntCallException
{
    public LocalException(
        bool isFatal, 
        short? messageId, 
        short? askId, 
        string message,
        Exception innerException  = null)
        :base(message, innerException)
    {
        IsFatal = isFatal;
        MessageId = messageId;
        AskId = askId;
    }

    public bool IsFatal { get; }
    public short? MessageId { get; }
    public short? AskId { get; }
}