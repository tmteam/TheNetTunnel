using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


namespace TheTunnel
{
	public class LightTunnelServer<TContract> where TContract: class, new()
	{
		Dictionary<TContract, LightTunnelClient<TContract>> contracts;
		public TContract[] Contracts{ 
			get {
				lock (contracts) {
					return contracts.Keys.ToArray ();
				}}}

		public LServer Server{ get; protected set; }

		public event delConnecterInfo<TContract> BeforeConnect;
		public event delConnecter<TContract> AfterConnect;
		public event delConnecter<TContract> OnDisconnect;

		public void Open(System.Net.IPAddress ip, int port)
		{
			if (Server != null)
				throw new InvalidOperationException ("Server is already open");
			contracts = new Dictionary<TContract, LightTunnelClient<TContract>> ();
			Server = new LServer ();
			Server.OnConnect+= server_onClientConnect;
			Server.OnDisconnect+= server_onClientDisconnect;
			Server.BeginListen(ip, port);

		}

		public void Close(){
			if (Server == null)
				throw new InvalidOperationException ("Server is already closed");
			Server.StopServer ();
			Server = null;
		}

		public LightTunnelClient<TContract> GetTunnel(TContract contract)
		{
			lock(contracts){
				if(!contracts.ContainsKey(contract))
					return null;
				else
					return contracts[contract];
			}
		}

		public void Kick(TContract contract){
			var tunnel = GetTunnel (contract);
			tunnel.Disconnect();
		}


		#region private 

		void server_onClientConnect (LServer sender, LClient newClient, ConnectInfo info)
		{
			var contract = new TContract ();
			var tunnel = new LightTunnelClient<TContract> (newClient, contract);

			lock (contracts) {
				contracts.Add (contract, tunnel);
			}

			if (BeforeConnect != null)
				BeforeConnect (this, contract, info);
			if (!info.AllowConnection)
				return;

			newClient.AllowReceive = true;

			if(AfterConnect!= null)
				AfterConnect(this, contract);
		}
	
		void server_onClientDisconnect (LServer server, LClient oldClient){
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
		#endregion
	}
	public delegate void delConnecterInfo<TContract>(LightTunnelServer<TContract> sender, TContract contract, ConnectInfo info) where TContract: class, new();
	public delegate void delConnecter<TContract>(LightTunnelServer<TContract> sender, TContract contract) where TContract: class, new();
}

