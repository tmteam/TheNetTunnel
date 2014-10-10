using System;
using System.Text;
using System.IO;

namespace TheTunnel.Serialization
{
	public class UnicodeSerializer: SerializerBase<string>{
		public UnicodeSerializer()
		{ Size = null;}

		public override void SerializeT (string obj, System.IO.MemoryStream stream)
		{
			StreamWriter sw = new StreamWriter (stream, Encoding.Unicode);
			sw.Write (obj);
			sw.Flush ();	
		}

	}
}

