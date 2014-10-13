using System;
using System.Net;
using System.Collections.Generic;
using TheTunnel;
namespace ChatServer
{
	class MainClass
	{
		static LightTunnelServer<ServerContract> server;

		public static void Main (string[] args)
		{
			string nick = "SERV";

			server = new TheTunnel.LightTunnelServer<ServerContract> ();

			server.BeforeConnect+= (sender, contract, info) => {
				contract.ReceiveMessage+= HandleReceiveMessage;
				Console.WriteLine("Client connected  ("+info.Client.Client.Client.RemoteEndPoint.ToString()+")"); 
			};

			server.AfterConnect += (sender, contract) => contract.SendMessage (DateTime.Now, "serv", "welcome to our superchat!");

			server.OnDisconnect+= (LightTunnelServer<ServerContract> sender, ServerContract contract) => {
                    Console.WriteLine("Client " + server.GetTunnel(contract).Client.Client.Client.RemoteEndPoint + " was disconnected");
					contract.ReceiveMessage-= HandleReceiveMessage;
			};

			Console.WriteLine ("Opening the server");
			server.OpenServer (IPAddress.Any, 4242);
			Console.WriteLine ("Waiting of clients");

			while (true) {
				var msg = Console.ReadLine();
				if(msg== "exit")
					return;
				foreach(var c in server.Contracts)	
					if(!c.SendMessage (DateTime.Now, nick, msg))
						Console.WriteLine ("Send failure");
			}

		}

		static void HandleReceiveMessage (DateTime arg1, string nick, string msg){
            Console.WriteLine(arg1.ToLongTimeString() + " " + nick + ": " + msg);
			foreach (var c in server.Contracts)
				if(!c.SendMessage (DateTime.Now, "ECHO-" + nick, msg))
					Console.WriteLine ("Send failure");
		}
	}
}
