using System;
using System.Linq;

namespace TNT.Cord.Serializers
{
    public class SequenceSerializer : ISerializer<object[]>
    {
        private readonly ISerializer[] serializers;
        private readonly bool singleMember = false;

        public SequenceSerializer(ISerializer[] serializers)
        {
            this.serializers = serializers;
            singleMember = serializers.Length == 1;
            if (serializers.Any(s => s.Size == null))
            {
                Size = null; //variable size
            }
            else
            {
                Size = serializers.Sum(s => s.Size.Value); //fixed size
            }
        }

        public void SerializeT(object[] obj, System.IO.MemoryStream stream)
        {
            for (int i = 0; i < obj.Length; i++) //Serializing one by one
            {
                if (serializers[i].Size.HasValue || singleMember)
                    serializers[i].Serialize(obj[i], stream);
                else
                {
                    var sPos = stream.Position;
                    stream.Write(new byte[] {0, 0, 0, 0}, 0, 4);
                    serializers[i].Serialize(obj[i], stream);

                    var len = BitConverter.GetBytes((int) (stream.Position - sPos - 4));
                    stream.Position = sPos;
                    stream.Write(len, 0, 4);
                    stream.Position = stream.Length;
                }
            }
        }

        public void Serialize(object obj, System.IO.MemoryStream stream)
        {
            SerializeT(obj as object[], stream);
        }

        public int? Size { get; protected set; }
    }
}