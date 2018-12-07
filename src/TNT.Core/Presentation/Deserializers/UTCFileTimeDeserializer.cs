using System;

namespace TNT.Presentation.Deserializers
{
	public class UTCFileTimeDeserializer: DeserializerBase<DateTime>
	{
		public UTCFileTimeDeserializer()
		{ Size = sizeof(long);}

		public override DateTime DeserializeT (System.IO.Stream stream, int size)
		{
			if (stream.Length - stream.Position < Size.Value)
				throw new Exception ("incorrect size");
			
			var arr = new byte[Size.Value];
			stream.Read (arr, 0, Size.Value);
			var lng = Tools.ToStruct<long> (arr, 0, Size.Value);
			return DateTime.FromFileTimeUtc (lng);
		}
	}
}

