using System;
using TheTunnel;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace Testing
{
	public class Test_FinalLightTunnel
	{
		public void PingPong()
		{
			///////////////SERVER///////////////////////

			var server = new LightTunnelServer<PingPongContract> ();

			var onConnect = new AutoResetEvent (false);

			server.BeforeConnect += (sender, contract, info) => {
				contract.ReceivePong+= (x, y) => {
					return new ProtoPoint{X = x, Y = y} ;
				};
				onConnect.Set();
			};

			var onDisconnect = new AutoResetEvent (false);
			server.OnDisconnect += (sender, contract) => {
				onDisconnect.Set();
			};
			server.OpenServer(IPAddress.Any, 6995);
			////////////CLIENT/////////////////////
			var ClientContract =new PingPongContract();
			ClientContract.ReceivePong+= (x, y) =>  {

				return new ProtoPoint{ X = x, Y = y};
			}; 
			var client = new LightTunnelClient<PingPongContract>();

			////////////CONNECTION/////////////////////
			client.Connect(IPAddress.Parse("127.0.0.1"),6995,ClientContract);

			if (!onConnect.WaitOne (1000))
				throw new Exception("not connected");

			////////////PING//////////////////
			var res = server.Contracts[0].SendPing(112,211);
			if (res == null)
				throw new Exception ("no answer");

			if(res.X!=112 || res.Y!= 211)
				throw new Exception("invalid data transmition");

			/////////////PONG////////////////////
			res = ClientContract.SendPing(10,11);
			if(res.X!=10 || res.Y!=11)
				throw new Exception("invalid data transmition");

			////////////CLOSE////////////////
			client.Disconnect();
			if(!onDisconnect.WaitOne(1000))
				throw new Exception("Client not disconnected");

			server.CloseServer ();

		}

		public void ManyConnections()
		{
			///////////////SERVER///////////////////////

			var server = new LightTunnelServer<PingPongContract> ();
			int connectedCount = 0;

			server.BeforeConnect += (sender, contract, info) => {
				contract.ReceivePong+= (x, y) => new ProtoPoint{X = x, Y = y};
			};

			server.AfterConnect +=  (sender, contract) => {
				connectedCount++;
				var rnd = Tools.rnd.Next();
				var res = contract.SendPing(rnd, rnd+1);
				if(res.X!=rnd || res.Y!= rnd+1)
					throw new Exception("server to client ping failure");
			};

			server.OnDisconnect += (sender, contract) => {
				connectedCount--;
			};
			server.OpenServer(IPAddress.Any, 6996);

			////////////CLIENTS/////////////////////
            List < LightTunnelClient < PingPongContract >> clients = new List<LightTunnelClient<PingPongContract>>();

			IPAddress ip = IPAddress.Parse ("127.0.0.1");
			int port = 6996;

			for (int k = 0; k < 10; k++) {

				#region Connect 20 clients
				for (int i = 0; i < 20; i++) {
					var ClientContract = new PingPongContract ();
					ClientContract.ReceivePong += (x, y) =>	new ProtoPoint{ X = x, Y = y };

					var client = new LightTunnelClient<PingPongContract> ();
					client.Connect (ip, port, ClientContract);
					clients.Add (client);

					var rnd = Tools.rnd.Next ();

					var res = ClientContract.SendPing (rnd, rnd + 1);
					if (res.X != rnd || res.Y != rnd + 1)
						throw new Exception ("client to server ping failure");
    			}
				#endregion

				#region send some flud
				foreach (var cl in clients) {
					var ppc = cl.CordDispatcher.Contract;
					var rnd = Tools.rnd.Next ();

					var res = ppc.SendPing (rnd, rnd + 1);
					if (res.X != rnd || res.Y != rnd + 1)
						throw new Exception ("client to server ping failure");
				}
				#endregion

				#region disconnect 20 clients
				for (int i = 0; i < 20; i++) {
					var client = clients [0];
					client.Disconnect ();
					clients.Remove (client);
				}
				#endregion
			
			}
			server.CloseServer ();
			int waitC = 0;
			while (server.Contracts.Length > 0 || connectedCount>0) {
				Thread.Sleep (10);
				waitC++;
				if(waitC>100)
					throw new Exception("clients are not disconnected");
			}

		}

		public void RecursionCall()
		{
			///////////////SERVER///////////////////////

			var server = new LightTunnelServer<PingPongContract> ();
			server.BeforeConnect +=  (sender, contract, info) => {
				contract.ReceivePong+= (x, y) => 
				{
					var ans =  contract.SendPing(x+1,y+1);
					ans.X++;
					ans.Y++;
					return ans;
				};
			};

			server.OpenServer(IPAddress.Any, 6997);


			///////////////CLIENT/////////////////////
			var ClientContract =new PingPongContract();
			ClientContract.ReceivePong+= (x, y) =>  {
				return new ProtoPoint{ X = x+1, Y = y+1};
			}; 
			var client = new LightTunnelClient<PingPongContract>();

			////////////CONNECTION/////////////////////
			client.Connect(IPAddress.Parse("127.0.0.1"),6997,ClientContract);
			var servAns = ClientContract.SendPing (1, 101);

			if (servAns.X != 4 || servAns.Y != 104)
				throw new Exception ("recursion check failed");
			client.Disconnect ();
		}

		public void CuteDDDOS()
		{
			var server = new LightTunnelServer<PingPongContract> ();
			server.BeforeConnect +=  (sender, contract, info) => {
				contract.ReceivePong+= (x, y) => 
				{
					return new ProtoPoint{ X = x+100, Y=y+100};
				};
			};
			server.OpenServer (IPAddress.Any, 6999);

			AutoResetEvent hundredDone = new AutoResetEvent (false);
			int doneCount = 0;
			for (int thread = 0; thread < 3; thread++) {
				ThreadPool.QueueUserWorkItem((m)=>
					{

						for (int num = 0; num < 100; num++) {
							///////////////CLIENT/////////////////////
							var ClientContract = new PingPongContract ();
							ClientContract.ReceivePong += (x, y) => {
								return new ProtoPoint{ X = x + 1, Y = y + 1 };
							}; 
							var client = new LightTunnelClient<PingPongContract> ();

							////////////CONNECTION/////////////////////
							client.Connect (IPAddress.Parse ("127.0.0.1"), 6999, ClientContract);
							var servAns = ClientContract.SendPing (1001, 1101);
							if (servAns.X != 1101 || servAns.Y != 1201)
								throw new Exception ("recursion check failed");
							client.Disconnect();
						}
						doneCount++;
						if(doneCount==2)
							hundredDone.Set();
					});
			}
			if(!hundredDone.WaitOne (60000))
				throw new Exception ("ddos was done succesfully ;(");
		}
	}
}

