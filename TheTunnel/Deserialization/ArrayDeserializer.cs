using System;
using System.Collections;
using System.Collections.Generic;

namespace TheTunnel
{
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

		public override bool TryDeserializeT(byte[] arr, int offset, out T obj, int length = -1){

			Array res;
			if (genericDeserializer.TryDeserialize (arr, offset, out res, length)) {
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
		bool TryDeserialize (byte[] array, int offset, out Array Tarray, int length = -1);
	}

	class ArrayGenericDeserializer<Telement>:IArrayGenericDeserializer 
	{
		public ArrayGenericDeserializer()
		{
			memberDeserializer = DeserializersFactory.Create (typeof(Telement)) as IDeserializer<Telement>;

			if (memberDeserializer.Size.HasValue) {
				FixSize = true;
				memberSize = memberDeserializer.Size.Value;

			}
		}
		IDeserializer<Telement> memberDeserializer;
		int memberSize;
		bool FixSize = false;

		public bool TryDeserialize (byte[] array, int offset, out Array Tarray, int lenght = -1)
		{
			if (FixSize)
				return TryDeserializeFix (array, offset,  out Tarray);
			else
				return TryDeserializeDyn (array, offset,  out Tarray, lenght);

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

		public bool TryDeserializeDyn(byte[] array, int offset,  out Array Tarray, int length = -1)
		{
			length = length == -1 ? array.Length - offset : length;
			Tarray = null;
			List<Telement> ans = new List<Telement> ();


			for (int i = offset;i< offset+length;) {

				Telement e;
				var eSize = BitConverter.ToInt32 (array, i);//Every element has 4byte size head
				i = i + 4;
				if (!memberDeserializer
					.TryDeserializeT (array, i, out e, eSize))
					return false;
				ans.Add (e);

				if (eSize == 0) {
					Tarray = null;
					return false;
				}
				i = i+ eSize;
			}
			Tarray = ans.ToArray ();
			return true;
		}
	}
}

