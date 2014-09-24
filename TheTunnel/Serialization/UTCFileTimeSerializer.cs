using System;

namespace TheTunnel
{
	public class UTCFileTimeSerializer: SerializerBase<DateTime>
	{
		public UTCFileTimeSerializer()
		{ Size = sizeof(long);}

		public override bool TrySerialize (DateTime obj, byte[] arr, int offset){
			var size = 8;
			if(arr==null|| offset+size> arr.Length)
				return false;
			Tools.SetToArray<long> (obj.ToFileTimeUtc(), arr, offset, size);
			return true;
		}

		public override byte[] Serialize (DateTime obj, int offset){
			byte[] ans = new byte[offset+ Size.Value];
			Tools.SetToArray<long>(obj.ToFileTimeUtc(), ans, offset, Size.Value);
			return ans;
		}
	}
}

