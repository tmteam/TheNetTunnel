using System;
using NUnit.Framework;
using TheTunnel;

namespace TestingQuant
{
	[TestFixture]
	public class qRandomStreamReceiving
	{
		[Test]
		public void Test()
		{
			Console.WriteLine ("qRandomReceiving started...");
			qReceiver receiver = new qReceiver ();
			Random rnd = new Random ();
			int percent = 0;
			int testlenght = 10000;
			Console.Write ("done: 0%");
			for (int i = 0; i < testlenght; i++) {
				var newpercent = (int)((i*100) / (double)testlenght);
				if (percent != newpercent) {
					Console.CursorLeft = 0;
					Console.Write ("done: " + newpercent + "%");
					percent = newpercent;
				}
				var size = rnd.Next () % 10000;
				var stream = new byte[size];
				rnd.NextBytes (stream);
				receiver.Set (stream);
			}
		}
	}
}

