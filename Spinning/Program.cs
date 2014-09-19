using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using ProtoBuf;
using System.IO;


namespace TheTunnel
{
	public class Program
	{

		static void Main(string[] args)
		{
			var ccc = new ChatClientContract2 ();
			CordDispatcher2 Dclient = new CordDispatcher2 (ccc);

			var csc = new ChatServerContract2 ();
			CordDispatcher2 Dserver = new CordDispatcher2 (csc);

			Dclient.NeedSend+= (arg1, arg2) =>
			{ 
				Console.WriteLine("Client Has Send "+ arg2.Length+" bytes");
				Dserver.Handle(arg2);
			};

			Dserver.NeedSend += (arg1, arg2) => {
				Console.WriteLine ("Server Has Send " + arg2.Length + " bytes");
				Dclient.Handle (arg2);
			};

			ccc.SendMessage (new mymsg2{ Sender = "tmt", Message = "c2s", SendTime = DateTime.Now });
			csc.SendToClient (new mymsg2{ Sender = "srv", Message = "s2c", SendTime = DateTime.Now });

			Console.WriteLine ("Asking..");
			var res = ccc.AskSmth (new mymsg2{ Sender = "tmt", Message = "Heeey, Tel me smth", SendTime = DateTime.Now });

			if(res==null)
				Console.WriteLine ("   silence...");
			else
				Console.WriteLine("   Got answer: "+ res.ReceiveTime+" handled: "+ res.Handled);

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
			Console.WriteLine ("Got ask from client: \r\n\t" + msg.SendTime + "\r\n\t" + msg.Sender + "\r\n\t" + msg.Message + "\r\n");
			return new sendResult2{ Handled = true, ReceiveTime = DateTime.Now }; 
		}
		[In(2)]	
		public void OnClientMessage(mymsg2 msg)
		{
			Console.WriteLine ("Got message from client: \r\n\t" + msg.SendTime + "\r\n\t" + msg.Sender + "\r\n\t" + msg.Message + "\r\n");
		}
		[Out(1)] public Action<mymsg2> SendToClient{get;set;}
	}

}