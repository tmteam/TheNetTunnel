using System;
using System.Linq;
namespace TheTunnel.Deserialization
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
		byte[] buffMemSize = new byte[4];
		public override object[] DeserializeT (System.IO.Stream stream, int size)
		{
			if (Types.Length == 1) 
				return new object[]{ deserializers [0].Deserialize (stream, size) };

			object[] ans = new object[Types.Length];
			int i = 0;


			foreach (var des in deserializers) {
				if (des.Size.HasValue)
					ans [i] = des.Deserialize (stream, des.Size.Value);
				else {
					stream.Read (buffMemSize, 0, 4);

					var mSize = BitConverter.ToInt32 (buffMemSize,0);
					if (mSize > (stream.Length - stream.Position))
						throw new Exception ("invalid sequence member size");
					ans [i] = des.Deserialize (stream, mSize);
				}
				i++;
			}
			return ans;
		}
	}
}

