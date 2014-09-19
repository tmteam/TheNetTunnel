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

		static void Main(string[] args)
		{
			var ccc = new ChatClientContract2 ();
			CordDispatcher2 d = new CordDispatcher2 (ccc);
			d.NeedSend+= (arg1, arg2) => Console.WriteLine("HasSend "+ arg2.Length+" bytes");
			ccc.SendMessage (new mymsg2{ Sender = "tmt", Message = "msg", SendTime = DateTime.Now });
			var res = ccc.AskQuetion(new mymsg2{ Sender = "tm", Message = "msg", SendTime = DateTime.Now });
		    Console.WriteLine(res==null);
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
	public class sendResult{
		[ProtoMember(1)] public bool Handled{get;set;}
		[ProtoMember(3)] public DateTime ReceiveTime{get;set;}
	}

	public class ChatClientContract2
	{
		[Out(2)] 
		public ddd SendMessage{get;set;}
		[Out(3)] 
		public Func<mymsg2,sendResult> AskQuetion{get;set;}


		public delegate void ddd(mymsg2 mm);
	}

	public class ChatServerContract	{
		[In(2)]	 public sendResult OnClientMessage(mymsg msg){/* handle client message here */ return new sendResult();}
		[Out(1)] public Action<mymsg> SendToClient{get;set;}
	}

}