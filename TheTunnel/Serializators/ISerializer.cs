using System;

namespace TheTunnel
{
	public interface ISerializer 
	{
		bool TrySerialize(object obj, int offset, out byte[] arr);
		bool TrySerialize(object obj, byte[] arr, int offset);
		byte[] Serialize(object obj, int offset);
	}
	public interface ISerializer<T>: ISerializer
	{
		bool TrySerialize(T obj, int offset, out byte[] arr);
		bool TrySerialize(T obj, byte[] arr, int offset);
		byte[] Serialize(T obj, int offset);
	}
}

