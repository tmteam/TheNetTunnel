using ProtoBuf;

namespace TNT.LocalSpeedTest.Contracts
{
    [ProtoContract]
    public class ProtoStruct
    {
        public ProtoStructItem[] Members { get; set; } 
    }
}