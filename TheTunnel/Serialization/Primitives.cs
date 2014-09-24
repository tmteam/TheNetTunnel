using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace TheTunnel
{
	public abstract class SerializerBase<T>:ISerializer<T>, ISerializer 
	{
		public int? Size{ get; protected set;}

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
		public int? Size{ get; protected set;}

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
		public PrimitiveSerializer()
		{
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

	public class PrimitiveDeserializer<T>:  DeserializerBase<T> {

		public PrimitiveDeserializer()
		{ 
			Size = Marshal.SizeOf (typeof(T));
		}
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
		public UnicodeSerializer()
		{ Size = null;}

		public override bool TrySerialize (string str, byte[] arr, int offset){
			Encoding.Unicode.GetBytes(str,0, str.Length, arr, offset);
			return true;
		}

		public override byte[] Serialize (string str, int offset){
			var size = Encoding.Unicode.GetByteCount (str);
			var ans = new byte[size + offset + 4];
			BitConverter.GetBytes (size).CopyTo (ans, offset);
			Encoding.Unicode.GetBytes(str,0, str.Length, ans, offset+4);
			return ans;
		}
			
	}

	public class UnicodeDeserializer: DeserializerBase<string>
	{
		public UnicodeDeserializer()
		{ Size = null;}

		public override bool TryDeserializeT (byte[] arr, int offset, out string str)
		{
			str = null;
			if (arr.Length < offset + 4)
				return false;
			int strByteLength = BitConverter.ToInt32 (arr, offset);
			if (arr.Length < offset + 4 + strByteLength)
				return false;
			str =  Encoding.Unicode.GetString (arr, offset+4, strByteLength);
			return true;
		}
	}

	public class UTCFileTimeSerializer: SerializerBase<DateTime>
	{
		public UTCFileTimeSerializer()
		{ Size = sizeof(long);}

		public override bool TrySerialize (DateTime obj, byte[] arr, int offset){
			var size = 8;
			if(arr==null|| offset+size> arr.Length)
				return false;
			Tools.SetToArray<long> (obj.ToFileTimeUtc(), arr, offset, size);
			return true;
		}

		public override byte[] Serialize (DateTime obj, int offset){
			byte[] ans = new byte[offset+ Size.Value];
			Tools.SetToArray<long>(obj.ToFileTimeUtc(), ans, offset, Size.Value);
			return ans;
		}
	}

	public class UTCFileTimeDeserializer: DeserializerBase<DateTime>
	{
		public UTCFileTimeDeserializer()
		{ Size = sizeof(long);}

		public override bool TryDeserializeT(byte[] arr, int offset, out DateTime obj){
			var size = 8;
			if (offset + size > arr.Length) {
				obj = DateTime.Now;
				return false;
			}
			var ftUTC = Tools.ToStruct<long> (arr, offset, size);
			obj = DateTime.FromFileTimeUtc (ftUTC).ToLocalTime();
			return true;
		}
	}
		
}
	