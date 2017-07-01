using System.Text;
using System.IO;

namespace TNT.Serialization
{
	public class UnicodeSerializer: SerializerBase<string>{
		public UnicodeSerializer()
		{ Size = null;}

		public override void SerializeT (string obj, System.IO.Stream stream)
		{
			var sw = new StreamWriter (stream, Encoding.Unicode);
			sw.Write (obj);
			sw.Flush ();	
		}
	}
}

