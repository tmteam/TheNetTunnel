using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using ProtoBuf;
using System.IO;
using System.Net;
using SomeContract;

namespace TheTunnel
{

	delegate void dt(int i, double d, DateTime dt, string str);
	public class Program
	{
		static void Main(string[] args)
		{


			var srv = new ChatServer ();
			srv.Open ();
			Console.Write ("Opening the tunnel to the server...");
			var client = new TcpClientTunnel ();

			var ccc = new ChatClientContract ();
			try
			{
				//Console.WriteLine("Press any key for connect");
				Console.ReadKey();
				client.Connect (new IPAddress(new byte[]{127,0,0,1}), 2049, ccc);
			}
			catch(Exception ex) {
				Console.WriteLine (ex.ToString ());
				Console.WriteLine (" Fail. Goodbye");
				return;
			}
		 	Console.WriteLine (" Connected");
			Console.Write (" registration...");

			UserInfo myself = new UserInfo{ FullName = "Sukhanov YP", Nick = "tmt" };
			var res = ccc.RegistrateMe (myself);

			if (res == null)
				Console.WriteLine ("..silence");
			else if (res.Result)
				Console.WriteLine ("Confirmed at " + res.TimeStamp.ToShortTimeString ());
			else {
				Console.WriteLine ("Rejected. Bye.");
				return;
			}

			Console.WriteLine ("write exit to close chat");
			while(true)
			{
				var cmd = Console.ReadLine ();
				if (cmd == "exit") {
					client.Disconnect ();
					return;
				}
				if(cmd.StartsWith("c "))
					{
						 var c = cmd.Remove (0, 2);
						 ccc.SendMessage (new Msg {
						 	User = myself,
						 	Timestamp = DateTime.Now,
						 	Message = c,
						});
					}
				else if (cmd.StartsWith("cc "))
					{
						var c = cmd.Remove (0, 3);
						var uis =ccc.Ask4UserList (c);
						if(uis==null)
							Console.WriteLine("nullget");
						else
						{
							Console.WriteLine("Got "+uis.Length);
							foreach(var r in uis)
								Console.WriteLine("\t"+r);

						}
					}
				else if(!srv.ParseCmd (cmd))
					return;

			}
		}
	}
}