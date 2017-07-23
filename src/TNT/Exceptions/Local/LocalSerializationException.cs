using System;
using TNT.Exceptions.Remote;

namespace TNT.Exceptions.Local
{
    public class LocalSerializationException : LocalException
    {
        public LocalSerializationException(
             short? messageId, short? askId,
             string message = null, Exception innerException = null)
            :base(true, messageId,askId,  message, innerException)
        {

        }
    }
}