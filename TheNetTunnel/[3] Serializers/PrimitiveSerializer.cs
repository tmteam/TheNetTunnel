using System.Runtime.InteropServices;
using System.IO;

namespace TNT.Serialization
{
	public class PrimitiveSerializer<T>:SerializerBase<T>
	{
		public PrimitiveSerializer()		{
			Size = Marshal.SizeOf(typeof(T));
		}

		public override void SerializeT (T obj, Stream stream){
			obj.WriteToStream (stream, Size.Value);
		}
	}		
}