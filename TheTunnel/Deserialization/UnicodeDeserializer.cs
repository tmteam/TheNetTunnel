using System;
using System.Text;

namespace TheTunnel
{
	public class UnicodeDeserializer: DeserializerBase<string>
	{
		public UnicodeDeserializer()
		{ Size = null;}

		public override bool TryDeserializeT (byte[] arr, int offset, out string str, int length = -1)
		{
			length = length == -1 ? arr.Length - offset : length;
			str = null;
			if (arr.Length <  offset + length)
				return false;
			str =  Encoding.Unicode.GetString (arr, offset, length);
			return true;
		}
	}


}

