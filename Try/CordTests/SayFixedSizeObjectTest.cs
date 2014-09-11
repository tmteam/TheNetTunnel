using System;
using NUnit.Framework;
using TheTunnel;

namespace TestingCord
{
	[TestFixture]
	public class SayFixedSizeObjectTest
	{
		[Test]
		public void Test ()
		{
			var Sender = new CordDispatcher ();
			var Receiver = new CordDispatcher ();

			Sender.NeedSend   += (CordDispatcher sender, byte[] msg) => Receiver.Parse(msg);
			Receiver.NeedSend += (CordDispatcher sender, byte[] msg) => Sender.Parse(msg);

			var testMsgSenderCord = new SayTestMessage ();
			var testMsgRecieverCord = new SayTestMessage ();

			Sender.AddCord (testMsgSenderCord);
			Receiver.AddCord (testMsgRecieverCord);

			testMessage msgReceived = null;

			testMsgRecieverCord.OnReceive += (ISayingCord<testMessage> sender, testMessage msg) => msgReceived = msg;

			testMessage sendingMsg = new testMessage {
				a = 10,
				b = 20,
				c = 30,
				d = 40,
			};

			testMsgSenderCord.Send (sendingMsg);

			if (!sendingMsg.AreEqual (msgReceived))
				throw new Exception ("messages are not equal");
		}
	}
}

