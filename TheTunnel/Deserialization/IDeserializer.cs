using System;

namespace TheTunnel
{
	public interface IDeserializer
	{
		int? Size{get;}
		bool TryDeserialize(byte[] arr, int offset, out object obj, int length = -1); 
	}
	public interface IDeserializer<T>: IDeserializer
	{
		bool TryDeserializeT(byte[] arr, int offset, out T obj, int length = -1); 
	}
}

