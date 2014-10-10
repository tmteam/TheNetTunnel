using System;

namespace TheTunnel.Serialization
{
	public class PrimitiveConvertSerializer<OriginT, ResultT >:SerializerBase<OriginT>{

		ISerializer<ResultT> primitive;

		public PrimitiveConvertSerializer(){
			primitive = new PrimitiveSerializer<ResultT>();
			Size = primitive.Size;
		}

		public override void SerializeT (OriginT obj, System.IO.MemoryStream stream)
		{
			primitive.Serialize (obj, stream);
		}
	}
}

