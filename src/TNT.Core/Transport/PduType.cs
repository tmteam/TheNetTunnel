namespace TNT.Transport
{
    public enum PduType: byte{
        Start = 1,
        Data = 2,
        AbortSending = 3,
    }
}