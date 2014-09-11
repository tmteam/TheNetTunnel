using System;
using NUnit.Framework;
using TheTunnel;
using System.Runtime.InteropServices;
using System.Linq;
namespace TestingCord
{
	[TestFixture]
	public class SayStringTest
	{
		[Test]
		public void Test ()
		{
			var clientA = new CordDispatcher ();
			var clientB = new CordDispatcher ();

			//Send messages dirrectly from one client to another
			clientA.NeedSend += (CordDispatcher sender, byte[] msg) 
				=> clientB.Parse(msg);
			clientB.NeedSend += (CordDispatcher sender, byte[] msg) 
				=> clientA.Parse(msg);


			var msgHandlingCord_A = new SayUTF32 ("amsg");
			var msgHandlingCord_B = new SayUTF32 ("amsg");

			clientA.AddCord (msgHandlingCord_A);
			clientB.AddCord (msgHandlingCord_B);

			string A_receiveBuff = null;
			string B_receiveBuff = null;

			msgHandlingCord_A.OnReceive += (ISayingCord<string> sender, string msg) 
				=> A_receiveBuff = msg;
			msgHandlingCord_B.OnReceive += (ISayingCord<string> sender, string msg) 
				=> B_receiveBuff = msg;

			string strBuff;

			Random rnd = new Random ();

			for (int i = 0; i < 1000; i++) {
				//Creating random string message with random lenght
				int msgLen = rnd.Next () % 2000;
				char[] charmsg = new char[msgLen];
				byte[] bytes = new byte[msgLen];

				rnd.NextBytes (bytes);

				for (int j = 0; j < msgLen; j++) {
					if(bytes[j]==0)//replacing endline symbols
						bytes[j] = 98;
					charmsg [j] = Convert.ToChar(bytes[j]);
				}

				A_receiveBuff = null;
				B_receiveBuff = null;

				strBuff = new string(charmsg);

				//Sending our message from A 2 B
				msgHandlingCord_A.Send (strBuff);

				if (!string.Equals(B_receiveBuff,strBuff))
					throw new Exception ("a message is lost at " + i);

				//Sending out message from B 2 B
				msgHandlingCord_B.Send (strBuff);

				if (!string.Equals(A_receiveBuff,strBuff))
					throw new Exception ("b message is lost at " + i);
			}	

		}
	}


}

