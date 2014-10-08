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


namespace SomeFTPlikeClient_Example
{

	class ServerContract{}
	public class Program
	{
		static void Main(string[] args)
		{

			LightTunnelClient client = new LightTunnelClient ();
			client.OnDisconnect += (sender, reason) => Console.WriteLine ("Disconnected. Reason: " + reason); ;

			var contract = new FileTransferClientContract ();
			Console.WriteLine ("Trying to connect...");

			while (true) {
				try {
					Console.Write("ip: ");
					var ip = Console.ReadLine();
					var ipprs = IPAddress.Parse(ip);
					Console.Write("port :");
					var port = int.Parse(Console.ReadLine());
					client.Connect (ipprs, port, contract);
					break;
				} catch (Exception ex) {
					Console.WriteLine ("Cannot connect because of " + ex.ToString());
					bool reconnect = false;
					while(true)
					{
						Console.WriteLine ("Try reconnect?[y/n]");
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

			var cc = new CmdCenter (contract);
			cc.RegistrateCmd (new GetCurrentDirrectory ());
			cc.RegistrateCmd (new GetDirContent ());
			cc.RegistrateCmd (new ChangeDirrectory ());
			cc.RegistrateCmd (new SendMessage ());
			cc.RegistrateCmd (new GetFullFile ());

			Console.WriteLine ("\r\nCommands:\r\n");

			foreach (var c in cc.Commands.Keys)
				Console.WriteLine("\t-"+c);

			while (true) {
				var input = Console.ReadLine ();
				if (input == "exit")
					break;
				else
					cc.RunCommand (input);
			}
			Console.WriteLine ("Bye");
			if(client.IsConnected)
				client.Disconnect ();
		}
	}

}