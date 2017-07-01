using System;
using System.IO;
using TNT.Cords;

namespace Testing
{
	public static class Tools
	{
		public static Random Rnd = new Random(DateTime.Now.Millisecond);

		public static MemoryStream GetRandomStream(int length)
		{
			var ans = new MemoryStream ();
			int maxArrSize = 1000;
			while (ans.Length < length) {
				var buff = new byte[maxArrSize];
				Rnd.NextBytes (buff);
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
        public static void ConnectContractsDirectly<Ta, Tb>(Ta A, Tb B)
            where Ta : class, new()
            where Tb : class, new()
        {
            var ACD = new CordDispatcher<Ta>(A);
            var BCD = new CordDispatcher<Tb>(B);

            ACD.NeedSend += (object arg1, System.IO.MemoryStream arg2) =>
            {
                arg2.Position = 0;
                BCD.Handle(arg2);
            };
            BCD.NeedSend += (object arg1, System.IO.MemoryStream arg2) =>
            {
                arg2.Position = 0;
                ACD.Handle(arg2);
            };

        }
	}
}

