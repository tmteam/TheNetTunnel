using System;
using System.IO;

namespace TheTunnel
{
	public interface ISerializer 
	{
		int? Size{get;}
		void Serialize(object obj, MemoryStream stream);
	}

	public interface ISerializer<T>: ISerializer
	{
		void SerializeT(T obj, MemoryStream stream);
	}
}

