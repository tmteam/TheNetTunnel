using System;

namespace TheTunnel.Serialization
{
	public class UTCFileTimeSerializer: SerializerBase<DateTime>
	{
		public UTCFileTimeSerializer()
		{ Size = sizeof(long);}

		public override void SerializeT (DateTime obj, System.IO.MemoryStream stream)
		{
			var lng = obj.ToFileTimeUtc ();
			Tools.WriteToStream<long> (lng, stream, Size.Value);
		}

	}
}

