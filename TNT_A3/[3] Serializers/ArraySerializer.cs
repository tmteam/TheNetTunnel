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
			}
		}

		ISerializer memberSerializer;
		bool isFix= false;
		Type memberType;

		public override void SerializeT (T obj, System.IO.MemoryStream stream)
		{
			if (isFix)
				SerializeFix (obj, stream);
			else
				SerializeDyn (obj, stream);
		}

		public void SerializeFix (T obj, System.IO.MemoryStream stream){
			var TArray = (obj as Array);

			for (int i = 0; i < TArray.Length; i++)
				memberSerializer.Serialize (TArray.GetValue (i), stream);
		}

		public void SerializeDyn (T obj, System.IO.MemoryStream stream)
		{
			var TArray = (obj as Array);

			for (int i = 0; i < TArray.Length; i++) 
			{
				var sPos = stream.Position;
				stream.Write (new byte[]{ 0, 0, 0, 0 },0,4);
				memberSerializer.Serialize(TArray.GetValue(i),stream);

				var len = BitConverter.GetBytes((int)(stream.Position - sPos-4));
				stream.Position = sPos;
				stream.Write (len, 0, 4);
				stream.Position = stream.Length;
			}
		}

	}
}

