using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace TheTunnel
{


	public class ArraySerializer<T>: SerializerBase<T>
	{
		public ArraySerializer()
		{
			memberType = typeof(T).GetElementType ();
			memberSize = Marshal.SizeOf (memberType);
			memberSerializer = SerializersFactory.GetSerializer (memberType);
		}
		ISerializer memberSerializer;
		Type memberType;
		int memberSize;

		public override bool TrySerialize (T obj, byte[] arr, int offset){
			var TArray = (obj as Array);
			if(arr==null|| offset+ TArray.Length* memberSize > arr.Length)
				return false;

			for (int i = 0; i < TArray.Length; i++) {
				if (!memberSerializer.TrySerialize (TArray.GetValue (i), arr, offset + i * memberSize))
					return false;
			}
			return true;
		}

		public override byte[] Serialize (T obj, int offset){
			var TArray = (obj as Array);
			byte[] ans = new byte[offset + TArray.Length * memberSize];
			for (int i = 0; i < TArray.Length; i++) 
				memberSerializer.TrySerialize (TArray.GetValue (i), ans, offset + i * memberSize);
			return ans;
		}
	}

	public class ArrayDeserializer<T>: DeserializerBase<T> where T: class, IEnumerable
	{
		public ArrayDeserializer()
		{
			var gt = typeof(ArrayGenericDeserializer<>).MakeGenericType (typeof(T).GetElementType ());
			genericDeserializer = Activator.CreateInstance (gt) as IArrayGenericDeserializer; 
		}
		IArrayGenericDeserializer genericDeserializer;

		public override bool TryDeserializeT(byte[] arr, int offset, out T obj){
		
			Array res;
			if (genericDeserializer.TryDeserialize (arr, offset, out res)) {
				obj = res as T;
				return true;
			} else {
				obj = default(T);
				return false;
			}
		}
	}

	interface IArrayGenericDeserializer
	{
		bool TryDeserialize (byte[] array, int offset, out Array Tarray);
	}

	class ArrayGenericDeserializer<Telement>:IArrayGenericDeserializer 
		where Telement: new()
	{
		public ArrayGenericDeserializer()
		{
			memberSize = Marshal.SizeOf (typeof(Telement));
			memberDeserializer = SerializersFactory.GetDeserializer (typeof(Telement)) as IDeserializer<Telement>;
		}
		IDeserializer<Telement> memberDeserializer;
		int memberSize;

		public bool TryDeserialize(byte[] array, int offset, out Array Tarray)
		{
			Tarray = null;
			int ansLenght = (array.Length - offset) / memberSize;
			Telement[] ans = new Telement[ansLenght];
			for (int i = 0; i < ansLenght; i++) {
				if (!memberDeserializer
					.TryDeserializeT (array, offset + i * memberSize, out ans [i]))
					return false;
			}
			Tarray = ans;
			return true;
		}
	}
}

