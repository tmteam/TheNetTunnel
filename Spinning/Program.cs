using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhAlpaTest
{
	class Program
	{
		static void Main(string[] args)
		{
			var res = new m2pTranslateTest().Test();
			Console.WriteLine("trans: "+ res);
			var res2 = new senderQueueTest().Test();
			Console.WriteLine("sendrecieve: " + res2);

			Console.ReadKey();
		}
	}
}
