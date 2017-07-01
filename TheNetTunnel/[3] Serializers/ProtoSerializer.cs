using ProtoBuf;

namespace TNT.Serialization
{
	public class ProtoSerializer<T>: SerializerBase<T>
	{
		public ProtoSerializer(){Size = null;}
		public override void SerializeT (T obj, System.IO.Stream stream)
		{
			ProtoBuf.Serializer.SerializeWithLengthPrefix<T>(stream, obj, PrefixStyle.Fixed32);	
		}

		public override void Serialize (object obj, System.IO.Stream stream)
		{
			SerializeT((T)obj, stream);
		}
	}
}

