using System;
using System.IO;
using System.Linq;
using TheTunnel;

namespace A3Expit
{
	public class De_Serialization
	{
		public void Primitive()
		{
			var io = 100;
			var ir = CheckAndRecreate (io);
			if (io != ir)
				throw new Exception ("io and ir are not equal");

			var bao = new byte[1000];
			var bar = CheckAndRecreate (bao);
			if (!bao.SequenceEqual (bar))
				throw new Exception ("bao and bar are not equal");
		}
		public static T CheckAndRecreate<T>(T origin)
		{
			int offs = 7;

			var ser = TheTunnel.SerializersFactory.Create (typeof(T));
			var deser = TheTunnel.DeserializersFactory.Create (typeof(T));
			MemoryStream stream = new MemoryStream();
			ser.Serialize (origin, stream);

			var serT = ser as ISerializer<T>;

			MemoryStream streamT = new MemoryStream ();

			serT.SerializeT (origin, streamT);

			var same = Tools.CompareStreams (stream, streamT);

			if (!same)
				throw new Exception ("serialized and seralizedT streams are not equal");

			stream.Position = 0;
			MemoryStream transportStream = new MemoryStream ();

			TheTunnel.Tools.CopyToAnotherStream (stream, transportStream, (int)stream.Length);

			transportStream.Position = 0;

			var deserialized = (deser as IDeserializer<T>).DeserializeT (transportStream, (int)transportStream.Length);

			return deserialized;

		}
	}
}

