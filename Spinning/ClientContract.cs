using System;
using TheTunnel;
namespace Spinning
{
	public class ClientContract
	{
		[Out(1)] public Action<string,string> SendMessage{get;set;}

		[In(2)]  public void ReceiveMessage(DateTime timeStamp, string sender, string message){
			Console.WriteLine ("["+timeStamp.ToShortTimeString()+"]" + sender + ": " + message);
		}
	}
}

