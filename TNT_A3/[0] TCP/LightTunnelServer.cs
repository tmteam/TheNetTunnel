using System;
using System.Collections.Generic;
using System.Linq;
namespace TheTunnel
{
	public class LightTunnelServer<TContract> where TContract: class, new()
	{
		Dictionary<TContract, LightTunnelClient> contracts;
		public TContract[] Contracts{ 
			get {
				lock (contracts) {
					return contracts.Keys.ToArray ();
				}}}

		public LightTcpServer Server{ get; protected set; }

		public void OpenServer(System.Net.IPAddress ip, int port)
		{
			if (Server != null)
				throw new InvalidOperationException ("Server is already open");
			contracts = new Dictionary<TContract, LightTunnelClient> ();
			Server = new LightTcpServer ();
			Server.OnConnect+= server_onClientConnect;
			Server.OnDisconnect+= server_onClientDisconnect;
			Server.BeginListen(ip, port);
		}

		public void CloseServer(){
			if (Server == null)
				throw new InvalidOperationException ("Server is already closed");
			Server.StopServer ();
			Server = null;
		}

		public LightTunnelClient GetTunnel(TContract contract)
		{
			lock(contracts)
			{
				if(!contracts.ContainsKey(contract))
					return null;
				else
					return contracts[contract];
			}
		}

		public event delConnecter<TContract> OnConnect;
		public event delConnecter<TContract> OnDisconnect;

		public void Kick(TContract contract)
		{
			var tunnel = GetTunnel (contract);
			tunnel.Disconnect();
		}

		void server_onClientConnect (LightTcpServer server, LightTcpClient newClient)
		{
			var contract = new TContract ();
			var tunnel = new LightTunnelClient (newClient, contract);
			lock (contracts) {
				contracts.Add (contract, tunnel);
			}
			if(OnConnect!= null)
				OnConnect(this, contract);
		}
		void server_onClientDisconnect (LightTcpServer server, LightTcpClient oldClient)
		{
			TContract client = null;
			lock (contracts) {
				client = contracts.FirstOrDefault (c => c.Value.Client == oldClient).Key;
				if (client != null) 
					contracts.Remove (client);
				else
					throw new Exception ();
			}
			if (OnDisconnect != null)
				OnDisconnect (this, client);
		}
	}
	public delegate void delConnecter<TContract>(LightTunnelServer<TContract> server, TContract contract) where TContract: class, new();
}

