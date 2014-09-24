using System;
using System.Text;

namespace TheTunnel
{
	public class UnicodeSerializer: SerializerBase<string>{
		public UnicodeSerializer()
		{ Size = null;}

		public override bool TrySerialize (string str, byte[] arr, int offset){
			Encoding.Unicode.GetBytes(str,0, str.Length, arr, offset);
			return true;
		}

		public override byte[] Serialize (string str, int offset){
			var size = Encoding.Unicode.GetByteCount (str);
			var ans = new byte[size + offset];
			Encoding.Unicode.GetBytes(str,0, str.Length, ans, offset);
			return ans;
		}

	}
}

