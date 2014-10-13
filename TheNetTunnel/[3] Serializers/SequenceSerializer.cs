using System;
using System.Collections.Generic;
using System.Linq;

namespace TheTunnel.Serialization
{
	public class SequenceSerializer: ISerializer<object[]>
	{
		public readonly Type[] Types;
		ISerializer[] serializers;
		bool singleMember = false;

		public SequenceSerializer (Type[] types)
		{
			this.Types = types;
			serializers = new ISerializer[types.Length];
			for (int i = 0; i < types.Length; i++) 
				serializers [i] = SerializersFactory.Create (types [i]);
			if (serializers.Any (s => s.Size == null))
				Size = null;
			else
				Size = serializers.Sum (s => s.Size.Value);
			singleMember = types.Length==1;
		}

		public void SerializeT (object[] obj, System.IO.MemoryStream stream)
		{
			for(int i = 0; i< obj.Length; i++)
			{
				if (serializers [i].Size.HasValue || singleMember)
					serializers [i].Serialize (obj [i], stream);
				else {
					var sPos = stream.Position;
					stream.Write (new byte[]{ 0, 0, 0, 0 }, 0, 4);
					serializers [i].Serialize (obj [i], stream);

					var len = BitConverter.GetBytes((int)(stream.Position - sPos-4));
					stream.Position = sPos;
					stream.Write (len, 0, 4);
					stream.Position = stream.Length;
				}
			}
		}

		public void Serialize (object obj, System.IO.MemoryStream stream)
		{
			SerializeT (obj as object[], stream);
		}
	
		public int? Size { get ; protected set; }
	}
}

