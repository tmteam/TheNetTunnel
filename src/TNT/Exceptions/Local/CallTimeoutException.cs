using System;

namespace TNT.Exceptions.Local
{
    public class CallTimeoutException: LocalException
    {
        public CallTimeoutException(short messageId, short askId)
            : base(false, messageId, askId, "Anwer timeout elasped", null)
        {
            
        }
    }
}
