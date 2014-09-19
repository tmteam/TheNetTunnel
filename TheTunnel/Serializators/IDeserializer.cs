using System;

namespace TheTunnel
{
	public interface IDeserializer
	{
		bool TryDeserialize(byte[] arr, int offset, out object obj); 
	}
	public interface IDeserializer<T>: IDeserializer
	{
		bool TryDeserializeT(byte[] arr, int offset, out T obj); 
	}
}

