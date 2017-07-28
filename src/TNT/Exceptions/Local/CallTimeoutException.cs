using System;

namespace TNT.Exceptions.Local
{
    public class CallTimeoutException: Exception
    {
        public short MessageId { get; }
        public short AskId { get; }

        public CallTimeoutException(short messageId, short askId)
            : base("Anwer timeout elasped", null)
        {
            MessageId = messageId;
            AskId = askId;
        }

    }
}
