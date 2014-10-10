using System;
using System.IO;

namespace A3Expit
{
	public static class Tools
	{
		public static Random rnd = new Random(DateTime.Now.Millisecond);

		public static MemoryStream GetRandomStream(int length)
		{
			MemoryStream ans = new MemoryStream ();
			int maxArrSize = 1000;
			while (ans.Length < length) {
				byte[] buff = new byte[maxArrSize];
				rnd.NextBytes (buff);
				ans.Write (buff, 0, (int)Math.Min (length-ans.Length, buff.Length));
			}
			return ans;
		}

		public static bool CompareStreams (MemoryStream a, MemoryStream b)
		{
			a.Position = 0;
			b.Position = 0;
			while (a.Position < a.Length) {
				var Bsended = a.ReadByte ();
				var Breceived = b.ReadByte ();
				if (Bsended != Breceived)
					return false;
			}
			return true;
		}
	}
}

