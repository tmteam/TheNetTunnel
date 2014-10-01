using System;

namespace TheTunnel
{
	public class PrimitiveConverterDeserializer<OriginT, ResultT>:IDeserializer<OriginT>
	{
		PrimitiveDeserializer<ResultT> primitive;
		public PrimitiveConverterDeserializer(){
			primitive = new PrimitiveDeserializer<ResultT>();

		}

		#region IDeserializer implementation

		public bool TryDeserializeT (byte[] arr, int offset, out OriginT obj, int length = -1)
		{
			ResultT outR;
			var res =  primitive.TryDeserializeT (arr, offset, out outR, length);
			if (res)
				obj = (OriginT)((object)outR);// (OriginT)Convert.ChangeType(outR, typeof(OriginT));
			else
				obj = default(OriginT);
			return res;
		}

		#endregion

		#region IDeserializer implementation

		public bool TryDeserialize (byte[] arr, int offset, out object obj, int length = -1)
		{
			return primitive.TryDeserialize (arr, offset, out obj, length);
		}

		public int? Size {
			get {
				return primitive.Size;
			}
		}

		#endregion
	}
}

