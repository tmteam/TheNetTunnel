using System;
using System.Linq;
namespace TheTunnel
{
	public class SequenceDeserializer:DeserializerBase<object[]>
	{
		public readonly Type[] Types;
		IDeserializer[] deserializers;
		public SequenceDeserializer (Type[] types){
			this.Types = types;
			deserializers = new IDeserializer[types.Length];
			for (int i = 0; i < types.Length; i++)
				deserializers [i] = DeserializersFactory.Create (types [i]);
			if (deserializers.Any (d => d.Size == null))
				Size = null;
			else
				Size = deserializers.Sum (d => d.Size.Value);
		}

		public override object[] DeserializeT (System.IO.Stream stream, int size)
		{
			if (Types.Length == 1) 
				return new object[]{ deserializers [0].Deserialize (stream, size) };

			object[] ans = new object[Types.Length];
			int i = 0;
			byte[] bMemSize = new byte[4];

			foreach (var des in deserializers) {
				if (des.Size.HasValue)
					ans [i] = des.Deserialize (stream, des.Size.Value);
				else {
					stream.Read (bMemSize, 0, 4);
					var mSize = BitConverter.ToInt32 (bMemSize,0);
					ans [i] = des.Deserialize (stream, mSize);
				}
				i++;
			}
			return ans;
		}
	}
}

