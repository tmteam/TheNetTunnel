using System;
using TNT.Exceptions.Remote;

namespace TNT.Exceptions.Local
{
    public class ConnectionIsLostException : LocalException
    {
        public ConnectionIsLostException(
           
            string message = null, short? messageId = null, short? askId = null, Exception innerException = null)
            :base(true, messageId,askId,  message, innerException)
        {

        }
    }
}