using System;

namespace TheTunnel
{
	public class ByteArrayDeserializer:IDeserializer<byte[]>
	{
		#region IDeserializer implementation

		public bool TryDeserializeT (byte[] arr, int offset, out byte[] obj, int length = -1)
		{
			length= length== -1? arr.Length-offset: length;
			if(arr.Length<offset+length)
			{
				obj = null;
				return false;
			}
			obj = new byte[length];	
			Array.Copy(arr,offset, obj,0, length);
			return true;
		}

		#endregion

		#region IDeserializer implementation

		public bool TryDeserialize (byte[] arr, int offset, out object obj, int length = -1)
		{
			byte[] ans = null;
			var res =  TryDeserializeT (arr, offset, out ans, length);
			obj = ans;
			return res;
		}

		public int? Size {	get { return null; }}
	
		#endregion
	}
}

