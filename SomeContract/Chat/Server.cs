using System;
using System.Collections.Generic;
using TheTunnel;
using System.Linq;
namespace SomeContract
{
	public class ChatServer
	{
		TcpServerTunnel<ChatServerContract> server;
		public  void Open ()
		{
			server = new TcpServerTunnel<ChatServerContract> ();
			server.OnConnect += onConnect;
			server.OnDisconnect+= onDisconnect;
			Console.WriteLine ("Opening the server");
			server.OpenServer (new System.Net.IPAddress (new byte[]{ 127, 0, 0, 1 }), 2049);
		}

		void onDisconnect (TcpServerTunnel<ChatServerContract> arg1, ChatServerContract arg2)
		{
			Console.WriteLine ("Client " + arg2.User.Nick + " was disconnected");
		}

		void onConnect (TcpServerTunnel<ChatServerContract> arg1, ChatServerContract arg2)
		{	Console.WriteLine ("Client Was Connected");
			arg2.OnMessage += OnMessage;
		}
		void OnMessage (ChatServerContract arg1, Msg msg)
		{	Console.WriteLine("Got message from client "+msg.Timestamp.ToShortTimeString () + " [" + msg.User.Nick + "] " + msg.Message);
			foreach (var c in server.ClientContracts) {
				msg.Message = "echo:" + msg.Message;
				c.SendMessage (msg);
			}
		}

		public bool ParseCmd(string msg)
		{
			if (msg == "exit")
				return false;
			else if(msg.StartsWith("kick "))
				{
					var spl = msg.Split(new char[]{' '});
					if(spl.Length>1)
					{
						var cl = server.ClientContracts.FirstOrDefault(c=>c.User.Nick == spl[1]);
						if(cl!=null)
						{
							cl.DisconnectUser(new DisconnectInfo
								{
									Initiator = new UserInfo {
									FullName  = "Server TT",
									Nick      = "srv"
								},
									Timestamp =DateTime.Now,
									Reason = DisconnectReason.Kick,
									Message = spl.Length>2?spl[2]: "thatslife" 
								});
							server.Kick(cl);
						}
					}
				}
			else if(msg.StartsWith("close "))
			{
				var spl = msg.Split(new char[]{' '});
				if(spl.Length>1)
				{
					var cl = server.ClientContracts.FirstOrDefault(c=>c.User.Nick == spl[1]);
					if(cl!=null)
						cl.RaiseDisconnection ();
				}
			}
				else
				{
					foreach (var c in server.ClientContracts) {
						var m = new Msg {
							User = new UserInfo {
								FullName = "Server TT",
								Nick = "srv"
							},
							Timestamp = DateTime.Now,
							Message = msg, 
						};
						c.SendMessage (m);
					
					}
				}
			return true;
		}
	}
}

