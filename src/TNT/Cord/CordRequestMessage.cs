namespace TNT.Cord
{
    public class CordRequestMessage
    {
        public CordRequestMessage(int id, int? askId, object[] arguments)
        {
            Arguments = arguments;
            Id = id;
            AskId = askId;
        }

        public int Id { get; }
        public int? AskId { get; }
        public object[] Arguments { get; }
    }
}