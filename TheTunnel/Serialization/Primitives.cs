using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace TheTunnel
{
	public abstract class SerializerBase<T>:ISerializer<T>, ISerializer 
	{
		public virtual bool TrySerialize (T obj, int offset, out byte[] arr)
		{
			arr = Serialize (obj, offset);
			return true;
		}

		public abstract bool TrySerialize (T obj, byte[] arr, int offset);

		public abstract byte[] Serialize (T obj, int offset);

		bool ISerializer.TrySerialize (object obj, byte[] arr, int offset){
			return TrySerialize ((T)obj, arr, offset);
		}

		bool ISerializer.TrySerialize (object obj, int offset, out byte[] arr)
		{
			return TrySerialize ((T)obj, offset, out arr);
		}

		byte[] ISerializer.Serialize (object obj, int offset)
		{
			return Serialize ((T)obj, offset);
		}
	}

	public abstract class DeserializerBase<T>:IDeserializer<T>
	{
		public abstract bool TryDeserializeT (byte[] arr, int offset, out T obj);

		public virtual bool TryDeserialize (byte[] arr, int offset, out object obj)
		{
			T des;
			var res = TryDeserializeT (arr, offset, out des);
			obj = des;
			return res;
		}
	}

	public class PrimitiveSerializer<T>:SerializerBase<T> 
	{
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

	public class PrimitiveDeserializer<T>:  DeserializerBase<T> where T: struct{
		public override bool TryDeserializeT(byte[] arr, int offset, out T obj){
			var size = Marshal.SizeOf(typeof(T));
			if (offset + size > arr.Length) {
				obj = default(T);
				return false;
			}
			obj = Tools.ToStruct<T> (arr, offset, size);
			return true;
		}
	}

	public class UnicodeSerializer: SerializerBase<string>{
		public override bool TrySerialize (string str, byte[] arr, int offset){
			var size = str.Length * 4 + offset + offset;
			if (size > arr.Length)
				return false;

			Encoding.Unicode.GetBytes(str,0, str.Length, arr, offset);
			return true;
		}

		public override byte[] Serialize (string str, int offset){
			byte[] ans = new byte[str.Length*4 + offset];
			Encoding.UTF32.GetBytes(str,0, str.Length, ans, offset);
			return ans;
		}
			
	}

	public class UnicodeDeserializer: DeserializerBase<string>
	{
		public override bool TryDeserializeT (byte[] arr, int offset, out string str)
		{
			str = null;
			//Check utf32 covertion possibility
			if ((arr.Length - offset) % 4 != 0)
				return false;
			else {
				str =  Encoding.Unicode.GetString (arr, offset, arr.Length - offset);
				return true;
			}
		}
	}

	public class UTCFileTimeSerializer: SerializerBase<DateTime>
	{
		public override bool TrySerialize (DateTime obj, byte[] arr, int offset){
			var size = 8;
			if(arr==null|| offset+size> arr.Length)
				return false;
			Tools.SetToArray<long> (obj.ToFileTimeUtc(), arr, offset, size);
			return true;
		}

		public override byte[] Serialize (DateTime obj, int offset){
			var size = Marshal.SizeOf(obj);
			byte[] ans = new byte[offset+ size];
			Tools.SetToArray<long>(obj.ToFileTimeUtc(), ans, offset, size);
			return ans;
		}
	}

	public class UTCFileTimeDeserializer: DeserializerBase<DateTime>
	{
		public override bool TryDeserializeT(byte[] arr, int offset, out DateTime obj){
			var size = 8;
			if (offset + size > arr.Length) {
				obj = DateTime.Now;
				return false;
			}
			var ftUTC = Tools.ToStruct<long> (arr, offset, size);
			obj = DateTime.FromFileTimeUtc (ftUTC);
			return true;
		}
	}
		
}
	