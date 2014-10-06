using System;
using System.Text;
using System.IO;

namespace TheTunnel
{
	public class UnicodeDeserializer: DeserializerBase<string>
	{
		public UnicodeDeserializer()
		{ Size = null;}

		public override string DeserializeT (System.IO.Stream stream, int size)
		{
			var ns = new MemoryStream (size);
			stream.CopyToAnotherStream (ns, size);
			var sr = new StreamReader (ns, Encoding.Unicode);
			return sr.ReadToEnd ();
		}
	}


}

