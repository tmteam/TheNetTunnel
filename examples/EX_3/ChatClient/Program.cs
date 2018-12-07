namespace EX_3_ChatClient
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var contract = new ClientContract ();
			contract.ReceiveMessage += (DateTime time, string nick, string msg) => {
				Console.WriteLine("["+time.ToLongTimeString()+"] "+nick+": "+ msg);
				return true;
			};

			TheTunnel.LightTunnelClient<ClientContract> client = null;

			while (true) 
			{
				Console.WriteLine ("Enter an ip:");
				var ip = Console.ReadLine ();
				var port = "4242";

				client = new TheTunnel.LightTunnelClient<ClientContract> ();
				client.OnDisconnect += (object sender, TheTunnel.DisconnectReason reason) => {
					Console.WriteLine ("Server Connection is closed. Reason: " + reason);
				};

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

		   while (true) {
				var msg = Console.ReadLine();
				if(msg== "exit")
					return;
				if (!client.IsConnected)
					return;
				contract.SendMessage(DateTime.Now, "TNT", msg);
			}
		}
	}
}
