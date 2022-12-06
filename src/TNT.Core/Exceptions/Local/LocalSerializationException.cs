using System;

namespace TNT.Exceptions.Local;

public class LocalSerializationException : TntCallException
{
    public LocalSerializationException(
        short? messageId, short? askId,
        string message = null, Exception innerException = null)
        :base(false, messageId, askId, message, innerException)
    {
    }
}