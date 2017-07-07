using System;
using System.Runtime.InteropServices;

namespace TNT.Cord.Deserializers
{
	public class ValueTypeDeserializer<T>: DeserializerBase<T> where T : struct
    {
		public ValueTypeDeserializer(){ 
			Size = Marshal.SizeOf (typeof(T));
		}

		public override T DeserializeT (System.IO.Stream stream, int size)
		{
			if (stream.Length - stream.Position < size) 
				throw new Exception ("Invalid size");
			
			var arr = new byte[Size.Value];
			stream.Read (arr, 0, Size.Value);
			return Tools.ToStruct<T> (arr, 0, Size.Value);
		}
	}
}

