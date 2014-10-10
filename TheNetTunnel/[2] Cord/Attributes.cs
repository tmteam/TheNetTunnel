using System;
using ProtoBuf;

namespace TheTunnel
{
	public class InAttribute: Attribute{
		public InAttribute(Int16 Id){ this.CordId = Id;} 
		public readonly Int16 CordId;
	}
	public class OutAttribute: Attribute{
		public OutAttribute(Int16 Id, UInt32 MaxAnswerAwaitInterval = 60000)
		{ 
			this.CordId = Id;
			this.MaxAnswerAwaitInterval = MaxAnswerAwaitInterval;
		} 
		public readonly Int16 CordId;
		public readonly UInt32 MaxAnswerAwaitInterval;
	}
}

