using System;
using TheTunnel;
using ProtoBuf;

namespace Try
{
	public class ProtoAskTest
	{
		public void Test()
		{
		TheTunnel.CordDispatcher sendSide = new CordDispatcher ();
		TheTunnel.CordDispatcher receiveSide = new CordDispatcher ();

		sendSide.NeedSend+= (CordDispatcher arg1, byte[] arg2) => receiveSide.Parse(arg2);
		receiveSide.NeedSend+= (CordDispatcher arg1, byte[] arg2) => sendSide.Parse(arg2);

		AskingProtocord<pChatSendMessage, pChatSendMessageResults> sendCord 
		= new AskingProtocord<pChatSendMessage, pChatSendMessageResults> ("smsg", "amsg");

		AskingProtocord<pChatSendMessage, pChatSendMessageResults> receiveCord 
		= new AskingProtocord<pChatSendMessage, pChatSendMessageResults> ("smsg", "amsg");

		sendSide.AddCord (sendCord);
		receiveSide.AddCord (receiveCord);

		receiveCord.OnAsking+= (ICord sender, pChatSendMessage arg) => {
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
				QuickAnswer = qAns,
			};
		};
		Console.WriteLine ("sendingMessage");
		pChatSendMessage sMsg = new pChatSendMessage {
			SenderName = "tmt",
			SendTime = DateTime.Now,
			Message = "Hi maan. It is first protocord message ever. It's [3] time when i'm trying to send recive some message via protobuff+ cord",
			VisibleTo = new string[]{"alex","tmt","hgena","warfollowme"},
			msgId = 42,
		};

		var ans = sendCord.Ask (sMsg, 10000);
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
}

[ProtoContract]
public class pChatSendMessage
{
	[ProtoMember(1)] public string SenderName{ get; set; }
	[ProtoMember(2)] public DateTime SendTime{get;set;}
	[ProtoMember(3)] public string Message{get;set;}
	[ProtoMember(4)] public string[] VisibleTo{get;set;}
	[ProtoMember(5)] public int msgId;

}

[ProtoContract]
public class pChatSendMessageResults
{
	[ProtoMember(1)] public string ReceivedBy{get;set;}
	[ProtoMember(2)] public DateTime ReceiveTime{get;set;}
	[ProtoMember(3)] public int msgId{get;set;}
	[ProtoMember(4)] public string QuickAnswer{ get; set; }
}
}

