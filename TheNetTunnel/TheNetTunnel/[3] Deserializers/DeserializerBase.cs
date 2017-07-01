using System;

namespace TheTunnel.Deserialization
{
	public abstract class DeserializerBase<T>: IDeserializer<T>
	{

		public abstract T DeserializeT (System.IO.Stream stream, int size);

		public virtual object Deserialize (System.IO.Stream stream, int size)
		{	return DeserializeT (stream, size);
		}

		public int? Size {	get;protected set;	}
	}
}

