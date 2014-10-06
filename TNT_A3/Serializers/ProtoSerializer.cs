using System;

namespace TheTunnel
{

	public class ProtoSerializer<T>: SerializerBase<T>
	{
		public ProtoSerializer(){Size = null;}
		public override void SerializeT (T obj, System.IO.MemoryStream stream)
		{
			ProtoBuf.Serializer.Serialize<T>(stream, obj);	
		}

		public override void Serialize (object obj, System.IO.MemoryStream stream)
		{
			ProtoBuf.Serializer.Serialize(stream, obj);	
		}

	}
}

