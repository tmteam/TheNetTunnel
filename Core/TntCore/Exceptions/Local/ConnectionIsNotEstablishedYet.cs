using TNT.Exceptions.Remote;

namespace TNT.Exceptions.Local
{
    
    public class ConnectionIsNotEstablishedYet: ConnectionIsLostException
    {
        public ConnectionIsNotEstablishedYet(
            string message = null, short? messageId = null, short? askId = null)
            :base(message,messageId, askId)
        {

        }
    }
}
