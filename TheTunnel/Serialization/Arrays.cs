using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

namespace TheTunnel
{


	public class ArraySerializer<T>: SerializerBase<T>
	{
		public ArraySerializer()
		{
			Size = null;
			memberType = typeof(T).GetElementType ();
			memberSerializer = SerializersFactory.GetSerializer (memberType);
			if (memberSerializer.Size.HasValue) {
				isFix = true;
				memberSize = memberSerializer.Size.Value;
			}
		}
		ISerializer memberSerializer;
		bool isFix= false;
		Type memberType;
		int memberSize;

		public override bool TrySerialize (T obj, byte[] arr, int offset)
		{
			if (isFix)
				return TrySerializeFix (obj, arr, offset);
			else
				return TrySerializeDyn (obj, arr, offset);
		}

		public bool TrySerializeFix (T obj, byte[] arr, int offset){
			var TArray = (obj as Array);
			if(arr==null|| offset+ TArray.Length* memberSize > arr.Length)
				return false;

			for (int i = 0; i < TArray.Length; i++) {
				if (!memberSerializer.TrySerialize (TArray.GetValue (i), arr, offset + i * memberSize))
					return false;
			}
			return true;
		}



		public bool TrySerializeDyn (T obj, byte[] arr, int offset)
		{
			var TArray = (obj as Array);

			if (arr == null)
				return false;

			int totalDataLenght = 0;

			for (int i = 0; i < TArray.Length; i++) 
			{
				var data  = memberSerializer.Serialize(TArray.GetValue(i),0);
				if (offset + data.Length > arr.Length)
					return false;
				data.CopyTo(arr,offset+totalDataLenght);
				totalDataLenght+=data.Length;
			}
			return true;
		}

		public override byte[] Serialize (T obj, int offset)
		{
			if (isFix)
				return SerializeFix (obj, offset);
			else
				return SerializeDyn (obj, offset);
		}

		public byte[] SerializeFix (T obj, int offset){
			var TArray = (obj as Array);
			byte[] ans = new byte[offset + TArray.Length * memberSize];
			for (int i = 0; i < TArray.Length; i++) 
				memberSerializer.TrySerialize (TArray.GetValue (i), ans, offset + i * memberSize);
			return ans;
		}

		public byte[] SerializeDyn (T obj, int offset)
		{
			var TArray = (obj as Array);

			byte[][] data = new byte[TArray.Length][];
			int totalDataLenght = 0;

			for (int i = 0; i < TArray.Length; i++) 
			{
				data[i]  = memberSerializer.Serialize(TArray.GetValue(i),0);
				totalDataLenght+=data[i].Length;
			}
			byte[] ans = new byte[totalDataLenght + offset];

			var offs = offset;

			for(int i = 0; i< TArray.Length; i++)
			{
				data[i].CopyTo(ans,offs);
				offs+=data[i].Length;
			}
			return ans;
		}

	}

		
	public class ArrayDeserializer<T>: DeserializerBase<T> where T: class, IEnumerable
	{
		public ArrayDeserializer()
		{
			Size = null;
			var eType = typeof(T).GetElementType ();
			var gt = typeof(ArrayGenericDeserializer<>).MakeGenericType (eType);
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
	{
		public ArrayGenericDeserializer()
		{
			memberDeserializer = SerializersFactory.GetDeserializer (typeof(Telement)) as IDeserializer<Telement>;

			if (memberDeserializer.Size.HasValue) {
				FixSize = true;
				memberSize = memberDeserializer.Size.Value;
			
			}
		}
		IDeserializer<Telement> memberDeserializer;
		int memberSize;
		bool FixSize = false;

		public bool TryDeserialize (byte[] array, int offset, out Array Tarray)
		{
			if (FixSize)
				return TryDeserializeFix (array, offset,  out Tarray);
			else
				return TryDeserializeDyn (array, offset,  out Tarray);
			
		}

		public bool TryDeserializeFix(byte[] array, int offset,  out Array Tarray)
		{
			Tarray = null;
			int ansLenght = (array.Length-offset) / memberSize;
			Telement[] ans = new Telement[ansLenght];
			for (int i = 0; i < ansLenght; i++) {
				if (!memberDeserializer
					.TryDeserializeT (array, offset + i * memberSize, out ans [i]))
					return false;
			}
			Tarray = ans;
			return true;
		}

		public bool TryDeserializeDyn(byte[] array, int offset,  out Array Tarray)
		{
			Tarray = null;
			List<Telement> ans = new List<Telement> ();


			for (int i = offset;i< array.Length;) {
			
				Telement e;
				if (!memberDeserializer
					.TryDeserializeT (array, i, out e))
					return false;
				ans.Add (e);
				var eSize = BitConverter.ToInt32 (array, i);//Every dynamic-sized object has 4byte size head

				if (eSize == 0) {
					Tarray = null;
					return false;
				}
				i = i+ eSize + 4;
			}
			Tarray = ans.ToArray ();
			return true;
		}
	}
}

