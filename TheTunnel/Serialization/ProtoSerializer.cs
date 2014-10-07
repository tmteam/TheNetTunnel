using System;

namespace TheTunnel
{

	public class ProtoSerializer: ISerializer
	{
		public int? Size{ get{ return null; }}

		public bool TrySerialize (object obj, byte[] arr, int offset){
			var res = ProtoTools.Serialize(obj, 0);

			if (res.Length > arr.Length + offset)
				return false;
			Array.Copy (res, 0, arr, offset, res.Length);
			return true;
		}

		public byte[] Serialize (object obj, int offset){
			var res = ProtoTools.Serialize(obj, offset);
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
		public override bool TryDeserializeT (byte[] arr, int offset, out T obj, int length = -1){
			length = length==-1? arr.Length-offset: length;
			if (arr.Length < offset + length) {
				obj = default(T);
				return false;
			}
			return ProtoTools.TryDeserialize (arr, offset, length, out obj);
		}
	}

}

