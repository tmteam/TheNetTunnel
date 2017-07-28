using System;
using TNT.Exceptions.Remote;

namespace TNT.Exceptions.Local
{
    public class LocalSerializationException : Exception
    {
        public LocalSerializationException(
             short? messageId, short? askId,
             string message = null, Exception innerException = null)
            :base(  message, innerException)
        {
            MessageId = messageId;
            AskId = askId;
        }
        public bool IsFatal => true;
        public short? MessageId { get; }
        public short? AskId { get; }
    }
}