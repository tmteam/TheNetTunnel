using System;

namespace TheTunnel
{
	public class ProtoDeserializer<T>: DeserializerBase<T>{
		public ProtoDeserializer()
		{
			Size = null;
		}

		public override T DeserializeT (System.IO.Stream stream, int size)
		{
			return ProtoBuf.Serializer.Deserialize<T> (stream);
		}
	}
}

