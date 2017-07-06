using System.IO;
using System.Runtime.InteropServices;

namespace TNT.Serializers
{
	public class PrimitiveSerializer<T>:SerializerBase<T>
	{
		public PrimitiveSerializer()		{
			Size = Marshal.SizeOf(typeof(T));
		}

		public override void SerializeT (T obj, MemoryStream stream){
			obj.WriteToStream (stream, Size.Value);
		}
	}		
}