using System;

namespace TheTunnel
{
	public class UTCFileTimeDeserializer: DeserializerBase<DateTime>
	{
		public UTCFileTimeDeserializer()
		{ Size = sizeof(long);}

		public override bool TryDeserializeT(byte[] arr, int offset, out DateTime obj, int length = -1){
			var size = 8;
			if (offset + size > arr.Length) {
				obj = DateTime.Now;
				return false;
			}
			var ftUTC = Tools.ToStruct<long> (arr, offset, size);
			obj = DateTime.FromFileTimeUtc (ftUTC).ToLocalTime();
			return true;
		}
	}
}

