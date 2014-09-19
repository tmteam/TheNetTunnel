using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using ProtoBuf;
using System.IO;
using Spinning;

namespace TheTunnel
{
	public class Program
	{
		static void proc(int i, string s)
		{

		}

		static void Main(string[] args)
		{

			var Dsend = new CordDispatcher ();
			var Dreceive = new CordDispatcher ();

			Dsend.NeedSend+= (CordDispatcher arg1, byte[] arg2) => Dreceive.Parse(arg2);
			Dreceive.NeedSend += (CordDispatcher arg1, byte[] arg2) => Dsend.Parse(arg2);

			Communicator sendcmc = new Communicator (Dsend);
			Communicator receivecmc = new Communicator (Dreceive);

			SenderContract s = new SenderContract ();

			ReceiverContract r = new ReceiverContract ();

			sendcmc.SetCordContract (s);

			receivecmc.SetCordContract (r);

			s.SendMsg (new mymsg{ Sender = "tmt", SendTime = DateTime.Now, Message = "HIIII" });
			Console.ReadKey ();
		}

		static void Client_OnReceive (qTcpClient arg1, qMsg arg2)
		{
			Console.WriteLine ("GotMsg: " + arg2.body.Length);
		}

		static void Client_OnDisconnect (qTcpClient obj)
		{
			Console.WriteLine ("Server has close a connection");
		}

		static void Server_OnConnect (qTcpServer arg1, qTcpClient arg2)
		{
			Console.WriteLine ("Client was succ connected to the server");

			Console.WriteLine ("SendingMessage...");

			byte[] arr = new byte[10000];
			for (int i = 0; i < arr.Length; i++) {
				arr [i] = (byte)(i % byte.MaxValue);
			}
			arg2.SendMessage (arr);
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
