namespace TNT.Presentation.Serializers
{
    public class ProtoSerializer<T> : SerializerBase<T>
    {
      
        public ProtoSerializer()
        {
            Size = null;
        }

        public override void SerializeT(T obj, System.IO.MemoryStream stream)
        {
            ProtoBuf.Serializer.SerializeWithLengthPrefix<T>(stream, obj, ProtoBuf.PrefixStyle.Fixed32 );
        }

        public override void Serialize(object obj, System.IO.MemoryStream stream)
        {
            SerializeT((T) obj, stream);
        }
    }
}

