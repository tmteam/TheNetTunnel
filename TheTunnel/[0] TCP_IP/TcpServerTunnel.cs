using System;
using System.Collections.Generic;
using System.Linq;
namespace TheTunnel
{
	public class TcpServerTunnel<TContract> where TContract: new()
	{
		Dictionary<TContract, TcpClientTunnel> clientContracts;

		public TContract[] ClientContracts{ get {return clientContracts.Keys.ToArray ();}}

		public qTcpServer Server{ get; protected set; }

		public void OpenServer(System.Net.IPAddress ip, int port)
		{
			clientContracts = new Dictionary<TContract, TcpClientTunnel> ();
			Server = new qTcpServer ();
			Server.OnConnect+= server_onClientConnect;
			Server.OnDisconnect+= server_onClientDisconnect;
			Server.BeginListen(ip, port);
		}

		public event Action<TcpServerTunnel<TContract>, TContract> OnConnect;
		public event Action<TcpServerTunnel<TContract>, TContract> OnDisconnect;

		public void Kick(TContract contract)
		{
			var tunnel = clientContracts [contract];
			tunnel.Disconnect();
		}
		void server_onClientConnect (qTcpServer arg1, qTcpClient arg2)
		{
			var contract = new TContract ();
			var tunnel = new TcpClientTunnel (arg2, contract);

			clientContracts.Add(contract, tunnel);

			if(OnConnect!= null)
				OnConnect(this, contract);
		}

		void server_onClientDisconnect (qTcpServer arg1, qTcpClient arg2)
		{
			var client = clientContracts.FirstOrDefault (c => c.Value.Client == arg2).Key;
			if (client != null) {
				clientContracts.Remove (client);
				if (OnDisconnect != null)
					OnDisconnect (this, client);
			} else
				throw new Exception ();
		}
	}
}

