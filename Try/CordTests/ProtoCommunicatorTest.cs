using System;
using TheTunnel;

namespace Try
{
	public class ProtoCommunicatorTest
	{
		public ProtoCommunicatorTest ()
		{
		}
		public void Test()
		{
			TheTunnel.CordDispatcher sendSide = new CordDispatcher ();
			TheTunnel.CordDispatcher receiveSide = new CordDispatcher ();

			sendSide.NeedSend+= (CordDispatcher arg1, byte[] arg2) => receiveSide.Parse(arg2);
			receiveSide.NeedSend+= (CordDispatcher arg1, byte[] arg2) => sendSide.Parse(arg2);

			Communicator sender = new Communicator (sendSide);
			Communicator receiver = new Communicator (receiveSide);


			var q = sender.GetResponder<pChatSendMessage, pChatSendMessageResults> ("qmsg", "amsg");
			receiver.SetResponder<pChatSendMessage, pChatSendMessageResults> ("qmsg", "amsg", answer);


			Console.WriteLine ("sendingMessage");

			pChatSendMessage sMsg = new pChatSendMessage {
				SenderName = "tmt",
				SendTime = DateTime.Now,
				Message = "Hi maan. It is first protocord message ever. It's [3] time when i'm trying to send recive some message via protobuff+ cord",
				VisibleTo = new string[]{"alex","tmt","hgena","warfollowme"},
				msgId = 42,
			};

			var ans = q(sMsg);

			if (ans == null)
				Console.WriteLine ("no answer");
			else
				Console.WriteLine("Got answer"+
					"\r\n\tQuickAns: "+ ans.QuickAnswer
					+"\r\n\tmsgId: "+ans.msgId
					+"\r\n\treceived by: "+ ans.ReceivedBy
					+"\r\n\treceive time: "+ ans.ReceiveTime
				);
		}
		public static pChatSendMessageResults answer(pChatSendMessage arg)
		{
			Console.WriteLine(
				"recived: "
				+"\r\n\tmsg: " + arg.Message 
				+"\r\n\tfrom: " + arg.SenderName
				+"\r\n\tsendTime: "+ arg.SendTime
				+"\r\n\tmsgId: "+ arg.msgId);
			if(arg.VisibleTo!=null)
				foreach (var a in arg.VisibleTo)
				{
					Console.WriteLine("\tvisible to: "+ a);
				}
			Console.Write("\r\n\tYour Quick Answer: ");
			var qAns = Console.ReadLine();
			return new pChatSendMessageResults
			{
				ReceiveTime = DateTime.Now,
				msgId = arg.msgId,
				ReceivedBy = "tmt",
			};
		}

	}
}

