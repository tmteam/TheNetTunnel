using System;

namespace TNT.Exceptions.Local;

public class ConnectionIsLostException : TntCallException
{
    public ConnectionIsLostException(
           
        string message = null, short? messageId = null, short? askId = null, Exception innerException = null)
        :base(true, messageId,askId,  message, innerException)
    {

    }
}