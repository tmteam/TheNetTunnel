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

		public override bool TrySerialize (T obj, byte[] arr, int offset){
			var size = Marshal.SizeOf(obj);
			if(arr==null|| offset+size> arr.Length)
				return false;
			Tools.SetToArray<T> (obj, arr, offset, size);
			return true;
		}

		public override byte[] Serialize (T obj, int offset){
			var size = Marshal.SizeOf(obj);
			byte[] ans = new byte[offset+ size];
			Tools.SetToArray<T>(obj, ans, offset, size);
			return ans;
		}
	}		
}
	