using System;
using TNT.Exceptions.Remote;

namespace TNT.Exceptions.Local
{
    public class ConnectionIsLostException : LocalException
    {
        public ConnectionIsLostException(
           
            string message = null, short? cordId = null, short? askId = null, Exception innerException = null)
            :base(true, cordId,askId,  message, innerException)
        {

        }
    }
}