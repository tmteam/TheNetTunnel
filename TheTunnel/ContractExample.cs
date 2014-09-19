using System;
using ProtoBuf;

namespace TheTunnel
{
	[ProtoContract]
	public class mymsg{
		[ProtoMember(1)] public string Sender{get;set;}
		[ProtoMember(2)] public string Message{ get; set;}
		[ProtoMember(3)] public DateTime SendTime{get;set;}
	}
	[ProtoContract]
	public class sendResult{
		[ProtoMember(1)] public bool Handled{get;set;}
		[ProtoMember(3)] public DateTime ReceiveTime{get;set;}
	}

	public class ChatClientContract	
	{
		[In(1)]	 
		public void OnMessage(mymsg msg){/* handle chat message here */}

		[Out(2)] 
		public Func<mymsg, sendResult> SendMessage{get;set;}
	}

	public class ChatServerContract	{
		[In(2)]	 public sendResult OnClientMessage(mymsg msg){/* handle client message here */ return new sendResult();}
		[Out(1)] public Action<mymsg> SendToClient{get;set;}
	}



































	public class InAttribute: Attribute{
		public InAttribute(Int16 Id){ this.CordId = Id;} 
		public readonly Int16 CordId;
	}
	public class OutAttribute: Attribute{
		public OutAttribute(Int16 Id){ this.CordId = Id;} 
		public readonly Int16 CordId;
	}
}

