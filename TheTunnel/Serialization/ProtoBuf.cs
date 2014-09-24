using System;

namespace TheTunnel
{

	public class ProtoSerializer: ISerializer
	{
		public int? Size{ get{ return null; }}

		public bool TrySerialize (object obj, byte[] arr, int offset){
			var res = ProtoTools.Serialize(obj, 0);
			if (res.Length > arr.Length + offset+ 4)
				return false;

			BitConverter.GetBytes (res.Length).CopyTo (arr, offset);

			Array.Copy (res, 0, arr, offset+4, res.Length);
			return true;
		}

		public byte[] Serialize (object obj, int offset){
			var res = ProtoTools.Serialize(obj, offset+4);
			BitConverter.GetBytes(res.Length-4-offset).CopyTo(res,offset);
			return res;
		}

		public bool TrySerialize (object obj, int offset, out byte[] arr)
		{
			arr = Serialize (obj, offset);
			return true;
		}
	}

	public class ProtoDeserializer<T>: DeserializerBase<T>{
		public ProtoDeserializer()
		{
			Size = null;
		}
		public override bool TryDeserializeT (byte[] arr, int offset, out T obj){
			if (arr.Length < offset + 4) {
				obj = default(T);
				return false;
			}
			var len = BitConverter.ToInt32 (arr, offset);
			if (len + offset > arr.Length) {
				obj = default(T);
				return false;
			}
			return ProtoTools.TryDeserialize (arr, offset+4, len, out obj);
		}
	}

}

