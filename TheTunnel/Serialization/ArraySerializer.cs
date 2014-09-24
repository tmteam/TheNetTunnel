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
			memberSerializer = SerializersFactory.Create (memberType);
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

				if (offset + data.Length + 4 > arr.Length)
					return false;

				BitConverter.GetBytes (data.Length).CopyTo (arr, offset + totalDataLenght);
				totalDataLenght += 4;

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
				totalDataLenght+=data[i].Length+4;
			}
			byte[] ans = new byte[totalDataLenght  + offset];

			var offs = offset;

			for(int i = 0; i< TArray.Length; i++)
			{
				BitConverter.GetBytes (data[i].Length).CopyTo (ans, offs);
				offs += 4;
				data[i].CopyTo(ans,offs);
				offs+=data[i].Length;
			}
			return ans;
		}

	}
}

