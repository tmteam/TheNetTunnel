using System;
using System.Linq;

namespace TNT.Serialization
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
				Size = null;//variable size
			else
				Size = serializers.Sum (s => s.Size.Value); //fixed size
			singleMember = types.Length==1;
		}

		public void SerializeT (object[] obj, System.IO.Stream stream)
		{
			for(var i = 0; i< obj.Length; i++)//Serializing one by one
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

		public void Serialize (object obj, System.IO.Stream stream){
			SerializeT (obj as object[], stream);
		}
	
		public int? Size { get ; protected set; }
	}
}

