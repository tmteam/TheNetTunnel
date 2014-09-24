using System;

namespace TheTunnel
{
	public class ByteArraySerializer: ISerializer<byte[]>
	{
		public bool TrySerialize (byte[] obj, int offset, out byte[] arr){
			arr = new byte[obj.Length + offset];
			Array.Copy (obj, 0, arr, offset, obj.Length);
			return true;
		}

		public bool TrySerialize (byte[] obj, byte[] arr, int offset){
			if (arr.Length < obj.Length + offset)
				return false;
			Array.Copy (obj, 0, arr, offset, obj.Length);
			return true;
		}

		public byte[] Serialize (byte[] obj, int offset){
			var arr = new byte[obj.Length + offset];
			Array.Copy (obj, 0, arr, offset, obj.Length);
			return arr;
		}

		public bool TrySerialize (object obj, int offset, out byte[] arr){
			return TrySerialize (obj as byte[], offset, out arr);
		}

		public bool TrySerialize (object obj, byte[] arr, int offset){
			return TrySerialize (obj as byte[], arr, offset);
		}

		public byte[] Serialize (object obj, int offset){
			return Serialize (obj as byte[], offset);
		}

		public int? Size {get {return null;	}}
	}
}

