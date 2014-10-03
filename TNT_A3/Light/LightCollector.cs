using System;

namespace TNT_A3
{
	using System;
	using System.IO;

	namespace TheTunnelA3
	{
		public class LightCollector
		{
			public void Set(byte[] packetFromAStream)
			{

			}
			public bool HasNewLightMessages{get{ return false; }}

			public void Next()
			{

			}
		}
		public class LightMessageAssembler
		{
			public void Initialize(int id)
			{
				this.id = id;
			}


			public DateTime latstTS;
			public int bytesDone;
			public MemoryStream stream;
			public int id;

		}
	}


}

