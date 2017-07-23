using TNT.Exceptions.Remote;

namespace TNT.Exceptions.Local
{
    public class ConnectionIsNotEstablishedYet: LocalException
    {
        public ConnectionIsNotEstablishedYet(
            string message = null, short? messageId = null, short? askId = null)
            :base(true, messageId,askId,  message)
        {

        }
    }
}
