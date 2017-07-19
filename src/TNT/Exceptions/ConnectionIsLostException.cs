using System;

namespace TNT.Exceptions
{
    public class ConnectionIsLostException : RemoteCallException
    {
        public ConnectionIsLostException(string message = null, Exception innerException = null)
            :base(true, RemoteCallExceptionId.ConnectionIsLostException, message)
        {

        }
    }
}