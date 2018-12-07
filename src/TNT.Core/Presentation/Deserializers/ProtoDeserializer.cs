using ProtoBuf;
using ProtoBuf.Meta;

namespace TNT.Presentation.Deserializers
{
	public class ProtoDeserializer<T>: DeserializerBase<T>
        where T: new()
    {
	    private TypeModel _model;

	    public ProtoDeserializer()
		{
			Size = null;

        }

		public override T DeserializeT (System.IO.Stream stream, int size)
		{
            var ans = ProtoBuf.Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Fixed32);
            return ans;
		}
	}
}

