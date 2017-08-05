using ProtoBuf;

namespace TNT.LocalSpeedTest.Contracts
{
    [ProtoContract]
    public class ProtoStruct
    {
        [ProtoMember(1)]
        public ProtoStructItem[] Members { get; set; } 
    }
}