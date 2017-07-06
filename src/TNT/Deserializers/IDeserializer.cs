using System.IO;

namespace TNT.Deserializers
{
    /// <summary>
    /// Interface for TNT type deserializer
    /// </summary>
	public interface IDeserializer
	{
        /// <summary>
        /// Size for fixedSized types, or null for variableSized types
        /// </summary>
		int? Size{get;}
        /// <summary>
        /// Create object from stream with max length of "size"
        /// </summary>
        /// <returns></returns>
		object Deserialize(Stream stream, int size);
	}

    /// <summary>
    /// Typed TNT deserializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public interface IDeserializer<T>: IDeserializer
	{
        /// <summary>
        /// Create typed object from stream with max length of "size"
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
		T DeserializeT(Stream stream, int size);
	}
}

