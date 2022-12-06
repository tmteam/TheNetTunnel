using ProtoBuf;

namespace TNT.SpeedTest.Contracts;

[ProtoContract]
public class ProtoStruct
{
    [ProtoMember(1)]
    public ProtoStructItem[] Members { get; set; } 
}