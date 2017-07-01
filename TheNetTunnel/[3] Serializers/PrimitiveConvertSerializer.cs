namespace TNT.Serialization
{
    /// <summary>
    /// PrimitiveSerializer<T> wrapperr. Allows to serialize object with type cast from Origin to Result"/>
    /// </summary>
    /// <typeparam name="OriginT"></typeparam>
    /// <typeparam name="ResultT"></typeparam>
	public class PrimitiveConvertSerializer<OriginT, ResultT >:SerializerBase<OriginT>{

		ISerializer<ResultT> primitive;

		public PrimitiveConvertSerializer(){
			primitive = new PrimitiveSerializer<ResultT>();
			Size = primitive.Size;
		}

		public override void SerializeT (OriginT obj, System.IO.Stream stream)
		{
			primitive.Serialize (obj, stream);
		}
	}
}

