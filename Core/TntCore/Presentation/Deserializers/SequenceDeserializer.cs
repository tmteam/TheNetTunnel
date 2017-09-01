using System;
using System.Linq;

namespace TNT.Presentation.Deserializers
{
	public class SequenceDeserializer:DeserializerBase<object[]>
	{
		IDeserializer[] deserializers;

        public SequenceDeserializer(IDeserializer[] deserializers)
        {
            this.deserializers = deserializers;
            if (deserializers.Any(d => d.Size == null))
                Size = null;
            else
                Size = deserializers.Sum(d => d.Size.Value);
        }

      
		public override object[] DeserializeT (System.IO.Stream stream, int size)
		{
            byte[] buffMemSize = new byte[4];

			if (deserializers.Length == 1) 
				return new object[]{ deserializers [0].Deserialize (stream, size) };

			object[] ans = new object[deserializers.Length];
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

