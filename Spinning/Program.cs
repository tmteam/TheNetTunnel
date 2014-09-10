using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spinning;
using System.Threading;

namespace TheTunnel
{
	class Program
	{
		static CordDispatcher A = new CordDispatcher ();
		static CordDispatcher B = new CordDispatcher ();
		static void Main(string[] args)
		{
			Console.WriteLine ("Simle QA testing");
			MsgWithA amsg = new MsgWithA ();
			MsgWithA bmsg = new MsgWithA ();

			A.AddCord (amsg);
			B.AddCord (bmsg);

			amsg.OnReceive+= (ISayingCord<string> sender, string msg) => {Console.WriteLine("a has Msg from b: "+ msg); };
			bmsg.OnReceive+= (ISayingCord<string> sender, string msg) => {Console.WriteLine("b has Msg from a: "+ msg); };

			A.NeedSend += (CordDispatcher sender, byte[] msg) => B.Parse(msg);
			B.NeedSend += (CordDispatcher sender, byte[] msg) => A.Parse(msg);

			Console.WriteLine ("Lets begin...\r\n");
			var res1 = amsg.Ask ("hiMaaan",1000);
			Console.WriteLine (res1 == null ? "no answer from b" : res1);

			var res2 = bmsg.Ask ("Lopata :)))", 1000);
			Console.WriteLine (res2 == null ? "no answer from a" : res2);
		}
	}
}
