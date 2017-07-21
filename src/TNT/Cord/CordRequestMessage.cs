namespace TNT.Cord
{
    public class CordRequestMessage
    {
        public CordRequestMessage(short id, short? askId, object[] arguments)
        {
            Arguments = arguments;
            Id = id;
            AskId = askId;
        }

        public short Id { get; }
        public short? AskId { get; }
        public object[] Arguments { get; }
    }
}