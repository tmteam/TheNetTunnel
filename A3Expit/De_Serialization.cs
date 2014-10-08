using System;
using System.IO;
using System.Linq;
using TheTunnel;
using System.Collections.Generic;

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

		}

		public void UTF()
		{
			string origin = @"suppose I should be upset, even feel violated, but I'm not. No, in fact, I think this is a friendly message, like ""Hey, wanna play?"" and yes, I want to play. I really, really do. ";
			var des = CheckAndRecreate (origin);
			if (des.CompareTo (origin)!=0)
				throw new Exception ("Sent and received strings are not equal");
		}

		public void FixedSizeArrays()
		{
			List<double> doubles = new List<double> ();
			for (int i = 0; i < 1000; i++) {
				doubles.Add (Tools.rnd.NextDouble ());
			}

			var origin = doubles.ToArray();
			var des = CheckAndRecreate (origin);

			if (!origin.SequenceEqual (des))
				throw new Exception ("Sent and received double[] are not equal");
		}

		public void DynamicSizeArrays()
		{
			string[] arr = new string[]{ "first", "second", "third" };
			var des = CheckAndRecreate (arr);
			if (!des.SequenceEqual (arr))
				throw new Exception ("Sent and received string[] are not equal ");
		}

		public static T CheckAndRecreate<T>(T origin)
		{
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

