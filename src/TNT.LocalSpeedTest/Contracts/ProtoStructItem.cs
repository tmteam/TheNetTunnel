using System;
using ProtoBuf;

namespace TNT.LocalSpeedTest.Contracts
{
    [ProtoContract]
    public class ProtoStructItem
    {
        [ProtoMember(0)]
        public int Integer { get; set; }
        [ProtoMember(1)]
        public long Long { get; set; }
        [ProtoMember(2)]
        public string Text { get; set; }
        [ProtoMember(3)]
        public byte Byte { get; set; }
        [ProtoMember(4)]
        public DateTime Time { get; set; }
        [ProtoMember(5)]
        public int[] IntegerArray { get; set; }

    }
}