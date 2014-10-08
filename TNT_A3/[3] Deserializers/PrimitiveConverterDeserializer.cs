using System;

namespace TheTunnel
{
	public class PrimitiveConverterDeserializer<OriginT, ResultT>:DeserializerBase<OriginT>
	{
		PrimitiveDeserializer<ResultT> primitive;

		public PrimitiveConverterDeserializer(){
			primitive = new PrimitiveDeserializer<ResultT>();
			Size = primitive.Size;
		}

		public override OriginT DeserializeT (System.IO.Stream stream, int size)
		{
			return (OriginT)primitive.Deserialize (stream, size);
		}
	}
}

