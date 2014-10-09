using System;
using System.Net;

namespace ChatClient
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ClientContract contract = null;
			TheTunnel.LightTunnelClient client = null;
			while (true) {
				Console.WriteLine ("Enter an ip:");
				var ip = Console.ReadLine ();
				var port = "4242";

				contract = new ClientContract ();

				client = new TheTunnel.LightTunnelClient ();
				client.OnDisconnect+= ClientOnDisconnect;
				try
				{
					client.Connect (IPAddress.Parse (ip), int.Parse (port), contract);
					break;
				}
				catch {
					Console.WriteLine ("Cannot connect to " + ip);
					Console.WriteLine ("try again? [y]");
					if (Console.ReadKey ().Key != ConsoleKey.Y) {
						Console.WriteLine ("Bye");
						return;
					}
				}
			}
			Console.WriteLine ("Succesfully connected");

			/*contract.ReceiveMessage += (DateTime sendtime, string  nck, string msg) => {
				Console.WriteLine(sendtime.ToLongTimeString()+" ["+nck+"]: "+ msg);
				return true;
			};*/

			while (true) {
				var msg = Console.ReadLine();
				if(msg== "exit")
					return;
				if (!client.IsConnected)
					return;
				contract.SendMessage(DateTime.Now, "TNT", msg);
			}
		
		}

		static void ClientOnDisconnect (TheTunnel.LightTunnelClient sender, TheTunnel.DisconnectReason reason)
		{
			Console.WriteLine ("Server Connection is closed.");
		}
	}
}
