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

            //var model = TypeModel.Create();
            //model.Add(typeof(T), true);
            //_model = model.Compile();
        }

		public override T DeserializeT (System.IO.Stream stream, int size)
		{
            //T ans = new T();
		     //_model.DeserializeWithLengthPrefix(stream, ans, typeof(T), PrefixStyle.Fixed32, 1);
		    //return ans;

		    var ans =  ProtoBuf.Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Fixed32);
		    return ans;
		}
	}
}

