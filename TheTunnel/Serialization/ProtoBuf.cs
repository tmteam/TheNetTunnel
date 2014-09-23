using System;

namespace TheTunnel
{

	public class ProtoSerializer: ISerializer
	{
		public bool TrySerialize (object obj, byte[] arr, int offset){
			var res = ProtoTools.Serialize(obj, 0);
			if (res.Length > arr.Length + offset)
				return false;
			Array.Copy (res, 0, arr, offset, res.Length);
			return true;
		}

		public byte[] Serialize (object obj, int offset){
			return ProtoTools.Serialize(obj, offset);
		}

		public bool TrySerialize (object obj, int offset, out byte[] arr)
		{
			arr = Serialize (obj, offset);
			return true;
		}
	}

	public class ProtoDeserializer<T>: DeserializerBase<T>{
		public override bool TryDeserializeT (byte[] arr, int offset, out T obj){
			return ProtoTools.TryDeserialize (arr, offset, out obj);
		}
	}

}

