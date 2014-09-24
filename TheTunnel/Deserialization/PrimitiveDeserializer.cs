using System;
using System.Runtime.InteropServices;

namespace TheTunnel
{
	public class PrimitiveDeserializer<T>:  DeserializerBase<T> {

		public PrimitiveDeserializer(){ 
			Size = Marshal.SizeOf (typeof(T));
		}

		public override bool TryDeserializeT(byte[] arr, int offset, out T obj, int length = -1){
			var size = Marshal.SizeOf(typeof(T));
			if (offset + size > arr.Length) {
				obj = default(T);
				return false;
			}
			obj = Tools.ToStruct<T> (arr, offset, size);
			return true;
		}
	}
}

