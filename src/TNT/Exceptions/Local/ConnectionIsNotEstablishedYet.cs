using TNT.Exceptions.Remote;

namespace TNT.Exceptions.Local
{
    public class ConnectionIsNotEstablishedYet: LocalException
    {
        public ConnectionIsNotEstablishedYet(
            string message = null, short? cordId = null, short? askId = null)
            :base(true, cordId,askId,  message)
        {

        }
    }
}
