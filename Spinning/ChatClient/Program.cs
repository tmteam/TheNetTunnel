using System;
using System.Net;

namespace ChatClient
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Press any key for connection...");
			Console.ReadKey ();
			string ip = "127.0.0.1";
			string port = "4242";

			string nick = "TNT";

			var contract = new ClientContract ();

			TheTunnel.LightTunnelClient client = new TheTunnel.LightTunnelClient ();
			client.Connect (IPAddress.Parse (ip), int.Parse (port), contract);

			Console.WriteLine ("Succesfully connected");

			contract.ReceiveMessage += (DateTime sendtime, string  nck, string msg) => {
				Console.WriteLine(sendtime.ToLongTimeString()+" ["+nck+"]: "+ msg);
				return true;
			};

			while (true) {
				var msg = Console.ReadLine();
				if(msg== "exit")
					return;
				contract.SendMessage(DateTime.Now, nick, msg);
			}
		
		}
	}
}
