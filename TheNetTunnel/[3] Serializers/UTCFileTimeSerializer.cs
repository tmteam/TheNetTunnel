using System;

namespace TNT.Serialization
{
	public class UTCFileTimeSerializer: SerializerBase<DateTime>
	{
		public UTCFileTimeSerializer()
		{ Size = sizeof(long);}

		public override void SerializeT (DateTime obj, System.IO.Stream stream)
		{
			var lng = obj.ToFileTimeUtc ();
			StaticTools.WriteToStream<long> (lng, stream, Size.Value);
		}

	}
}

