namespace TNT.Presentation
{
    public class RequestMessage
    {
        public RequestMessage(short typeId, short? askId, object[] arguments)
        {
            Arguments = arguments;
            TypeId = typeId;
            AskId = askId;
        }

        public short TypeId { get; }
        public short? AskId { get; }
        public object[] Arguments { get; }
    }
}