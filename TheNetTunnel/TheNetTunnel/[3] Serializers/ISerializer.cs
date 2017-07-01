using System;
using System.IO;

namespace TheTunnel.Serialization
{
    /// <summary>
    /// Interface for SomeType Serializer
    /// </summary>
	public interface ISerializer 
	{
        /// <summary>
        /// Size for fixedSized types, or null for variableSized types
        /// </summary>
		int? Size{get;}
        /// <summary>
        /// Serialize object obj, and put result bytes at end of stream
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
		void Serialize(object obj, MemoryStream stream);
	}
    
    /// <summary>
    /// Typed Serializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public interface ISerializer<T>: ISerializer
	{
        /// <summary>
        /// Serialize object obj and put reslut bytes at end of stream
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
		void SerializeT(T obj, MemoryStream stream);
	}
}

