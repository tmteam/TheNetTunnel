using System;
using TheTunnel;

namespace ChatClient
{
	public class ClientContract
	{
		[Out(1)] public Action<DateTime, string, string> SendMessage{get;set;}
		//[In(2)]  public event Func<DateTime, string, string, bool> ReceiveMessage;
		[In(2)] public bool ReceiveMessage(DateTime dt, string nick, string msg)
		{
			Console.WriteLine ("bla bla");
			return 	true;
		}
	}

}

