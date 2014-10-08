using System;
using System.Net;
using System.Collections.Generic;
namespace ChatServer
{
	class MainClass
	{
		public static List<ServerContract> clients = new List<ServerContract> ();
		public static void Main (string[] args)
		{
			string nick = "SERV";

			var server = new TheTunnel.LightTunnelServer<ServerContract> ();

			server.OnConnect+= (TheTunnel.LightTunnelServer<ServerContract> sender, ServerContract contract) => {
				lock(clients)
				{
					clients.Add(contract);
					contract.ReceiveMessage+= HandleReceiveMessage;
				}
			};

			server.OnDisconnect+= (TheTunnel.LightTunnelServer<ServerContract> sender, ServerContract contract) => {
				lock(clients)
				{
					clients.Remove(contract);
					contract.ReceiveMessage-= HandleReceiveMessage;
				}
			};

			server.OpenServer (IPAddress.Any, 4242);
			while (true) {
				var msg = Console.ReadLine();
				if(msg== "exit")
					return;
				lock (clients) {
					foreach (var c in clients) {
						c.SendMessage (DateTime.Now, nick, msg);
					}
				}
			}

		}

		static void HandleReceiveMessage (DateTime arg1, string nick, string msg)
		{
			lock (clients) {
				foreach (var c in clients) 
					c.SendMessage (DateTime.Now, "ECHO-"+nick, msg);
			}
		}
	}
}
