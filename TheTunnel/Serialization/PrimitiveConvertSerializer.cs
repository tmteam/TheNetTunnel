using System;

namespace TheTunnel
{
	public class PrimitiveConvertSerializer<OriginT, ResultT >:ISerializer<OriginT>{
		public PrimitiveConvertSerializer(){
			primitive = new PrimitiveSerializer<ResultT>();
		}
		PrimitiveSerializer<ResultT> primitive;
		#region ISerializer implementation
		public bool TrySerialize (OriginT obj, int offset, out byte[] arr){
			return (primitive as ISerializer).TrySerialize (obj, offset, out arr);
		}

		public bool TrySerialize (OriginT obj, byte[] arr, int offset){
			return (primitive as ISerializer).TrySerialize (obj, arr, offset);
		}

		public byte[] Serialize (OriginT obj, int offset){
			return (primitive as ISerializer).Serialize (obj, offset);
		}

		#endregion
		#region ISerializer implementation
		public bool TrySerialize (object obj, int offset, out byte[] arr)
		{
			return (primitive as ISerializer).TrySerialize (obj, offset, out arr);
		}
		public bool TrySerialize (object obj, byte[] arr, int offset)
		{
			return (primitive as ISerializer).TrySerialize (obj, arr, offset);
		}
		public byte[] Serialize (object obj, int offset)
		{
			return (primitive as ISerializer).Serialize (obj, offset);
		}
		public int? Size {
			get {
				return primitive.Size;
			}
		}
		#endregion
	
	}
}

