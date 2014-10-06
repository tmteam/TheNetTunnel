using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace TheTunnel
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