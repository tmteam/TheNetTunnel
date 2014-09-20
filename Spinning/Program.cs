using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using ProtoBuf;
using System.IO;
using System.Net;


namespace TheTunnel
{
	public class Program
	{
		static void Main(string[] args)
		{
			var server = new TcpServerTunnel<ChatServerContract2>  ();
			server.OnConnect+= (arg1, arg2) => Console.WriteLine("IncomeConnection");

			var client = new TcpClientTunnel ();
			Console.WriteLine ("Opening the server tunnel");
			server.OpenServer (new IPAddress(new byte[]{127,0,0,1}), 2048);

			var ccc = new ChatClientContract2 ();

			Console.WriteLine ("Connecting via client tunnel");
			client.Connect (new IPAddress(new byte[]{127,0,0,1}), 2048, ccc);
			Console.WriteLine ("Connected");

			Console.WriteLine ("Sending primitive message");
			for (int i = 0; i < 10; i++) {
				ccc.SendMessage (new mymsg2{ Sender = "tmt", Message = "c2s"+ (i+1).ToString(), SendTime = DateTime.Now });
			}
			Console.WriteLine ("Asking..");
				var res = ccc.AskSmth (new mymsg2{ Sender = "tmt", Message = "Heeey, Tel me smth ", SendTime = DateTime.Now });

				if (res == null)
					Console.WriteLine ("   silence...");
				else
					Console.WriteLine ("\tGot answer: " + res.ReceiveTime + " handled: " + res.Handled);
			Console.ReadKey ();
		}
	}

	[ProtoContract]
	public class mymsg2{
		[ProtoMember(1)] public string Sender{get;set;}
		[ProtoMember(2)] public string Message{ get; set;}
		[ProtoMember(3)] public DateTime SendTime{get;set;}
	}
	[ProtoContract]
	public class sendResult2{
		[ProtoMember(1)] public bool Handled{get;set;}
		[ProtoMember(3)] public DateTime ReceiveTime{get;set;}
	}

	public class ChatClientContract2
	{
		[Out(3)] public Func<mymsg2, sendResult2> AskSmth{get;set;}
		[Out(2)] public ddd SendMessage{get;set;}
		[In(1)] public void OnServerMessage(mymsg2 msg)
		{
			Console.WriteLine ("Got message from server: \r\n\t" + msg.SendTime + "\r\n\t" + msg.Sender + "\r\n\t" + msg.Message + "\r\n");
		}

		public delegate void ddd(mymsg2 mm);
	}

	public class ChatServerContract2	{
		[In(3)]
		public sendResult2 AnswerSmth(mymsg2 msg)
		{
			num++;
			Console.WriteLine (num.ToString()+") Got ask from client: \r\n\t" + msg.SendTime + "\r\n\t" + msg.Sender + "\r\n\t" + msg.Message + "\r\n");
			return new sendResult2{ Handled = true, ReceiveTime = DateTime.Now }; 
		}
		int num = 0;
		[In(2)]	
		public void OnClientMessage(mymsg2 msg)
		{
			num++;

			Console.WriteLine (num.ToString()+") Got message from client: \r\n\t" + msg.SendTime + "\r\n\t" + msg.Sender + "\r\n\t" + msg.Message + "\r\n");
		}
		[Out(1)] public Action<mymsg2> SendToClient{get;set;}
	}

}