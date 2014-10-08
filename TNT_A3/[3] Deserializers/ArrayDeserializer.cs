using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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

		#region implemented abstract members of DeserializerBase

		public override T DeserializeT (System.IO.Stream stream, int size)
		{
			return (T)genericDeserializer.Deserialize (stream, size);
		}

		#endregion

		IArrayGenericDeserializer genericDeserializer;

	}

	interface IArrayGenericDeserializer
	{
		object Deserialize (Stream stream,  int length);
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

		public object  Deserialize (Stream stream,  int lenght)
		{
			if (FixSize)
				return DeserializeFix (stream, lenght);
			else
				return DeserializeDyn (stream, lenght);
		}

		 Telement[] DeserializeFix(Stream stream, int lenght)
		{
			int ansLenght = (lenght) / memberSize;
			Telement[] ans = new Telement[ansLenght];
			for (int i = 0; i < ansLenght; i++)
				ans[i] =memberDeserializer.DeserializeT (stream, memberSize);
			return ans;
		}

		 Telement[] DeserializeDyn(Stream stream, int lenght)
		{
			List<Telement> ans = new List<Telement> ();
			byte[] arr = new byte[4];
			int sPos = (int)stream.Position;
			while(stream.Position< sPos+lenght) {

				stream.Read (arr, 0, 4);

				var eSize = BitConverter.ToInt32 (arr,0);//Every element has 4byte size head
				if (eSize > stream.Length - stream.Position)
					throw new Exception ("invalid array member size");

				var e = memberDeserializer.DeserializeT (stream, eSize);
			
				ans.Add (e);

			}
			return ans.ToArray ();
		}
	}
}

