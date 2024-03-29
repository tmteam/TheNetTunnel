using System;
using System.IO;
using System.Text;

namespace TNT.Presentation.Deserializers;

public class UnicodeDeserializer: DeserializerBase<string>
{
	    
	public UnicodeDeserializer()
	{ Size = null;}

	public override string DeserializeT (System.IO.Stream stream, int size)
	{
		if (size == 0)
			return null;
		var ns = new MemoryStream(size);
		stream.CopyToAnotherStream(ns, size);
		ns.Position = 0;

		if (size == 2)
		{
			//check for EndOfString symbol (U+0003)
			if(ns.ReadShort()== 0x0003)
				return String.Empty;
			else
				ns.Position = 0;
		}
          
            
		var sr = new StreamReader (ns, Encoding.Unicode);
		return sr.ReadToEnd ();
	}
}