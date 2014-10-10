using System;
using TheTunnel;

namespace ChatServer
{
	public class ServerContract
	{
			[In(1)]  public event Action<DateTime, string, string> ReceiveMessage;
			[Out(2)] public Func<DateTime, string, string, bool> SendMessage{get;set;}
	}
}

