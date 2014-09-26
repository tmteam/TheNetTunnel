using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using ProtoBuf;
using System.IO;
using System.Net;
using TheTunnel;


namespace Spinning
{
	public class Program
	{
		static void Main(string[] args)
		{
			TcpClientTunnel client = new TcpClientTunnel ();
			client.OnDisconnect += (sender, reason) => Console.WriteLine ("Disconnected. Reason: " + reason); ;
			ClientContract contract = new ClientContract ();
			Console.WriteLine ("Press any key to connect");
			Console.ReadKey ();
			Console.WriteLine ("Trying to connect...");
			while (true) {
				try {
					client.Connect (new IPAddress (new byte[]{ 172, 16, 31, 34 }), 1234, contract);
					break;
				} catch (Exception ex) {
					Console.WriteLine ("Cannot connect because of " + ex.Message);
					bool reconnect = false;
					while(true)
					{
						Console.WriteLine ("Reconnect?[y/n]");
						var rc = Console.ReadKey ();
						if (rc.Key == ConsoleKey.Y) {
							reconnect = true;
							break;
						} else if(rc.Key== ConsoleKey.N) {
							reconnect = false;
							break;
						}
					}
					if (!reconnect) {
						Console.WriteLine ("Bye");
						return;
					}
				}
			}
				Console.WriteLine ("Succesfully to connected!");

			while (true) {
				var msg = Console.ReadLine ();
				if (msg == "exit")
					return;
				contract.SendMessage ("tmt", msg);
			}
		}
	}

}