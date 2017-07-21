using System;

namespace TNT.Exceptions.Local
{
    public class CallTimeoutException: LocalException
    {
        public CallTimeoutException(short cordId, short askId)
            : base(false, cordId, askId, "Anwer timeout elasped", null)
        {
            
        }
    }
}
