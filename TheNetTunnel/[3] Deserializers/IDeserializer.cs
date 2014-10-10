using System;
using System.IO;

namespace TheTunnel.Deserialization
{
	public interface IDeserializer
	{
		int? Size{get;}
		object Deserialize(Stream stream, int size);
	}

	public interface IDeserializer<T>: IDeserializer
	{
		T DeserializeT(Stream stream, int size);
	}
}

