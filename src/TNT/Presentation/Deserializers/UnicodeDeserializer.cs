using System.IO;
using System.Text;

namespace TNT.Presentation.Deserializers
{
	public class UnicodeDeserializer: DeserializerBase<string>
	{
		public UnicodeDeserializer()
		{ Size = null;}

		public override string DeserializeT (System.IO.Stream stream, int size)
		{
			var ns = new MemoryStream (size);
			stream.CopyToAnotherStream (ns, size);
			ns.Position = 0;
			var sr = new StreamReader (ns, Encoding.Unicode);
			return sr.ReadToEnd ();
		}
	}
}

