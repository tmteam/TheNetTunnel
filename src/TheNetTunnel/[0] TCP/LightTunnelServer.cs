using System;
using System.Collections.Generic;
using System.Linq;

namespace TheTunnel
{
    public class LightTunnelServer<TContract> where TContract : class, new()
    {
        private Dictionary<TContract, LightTunnelClient<TContract>> contracts;

        /// <summary>
        /// Contracts that associated with current connected clients 
        /// </summary>
        public TContract[] Contracts
        {
            get
            {
                lock (contracts)
                {
                    return contracts.Keys.ToArray();
                }
            }
        }

        /// <summary>
        /// Light server object
        /// </summary>
        public LServer Server { get; protected set; }

        /// <summary>
        /// Raising before client connection done
        /// </summary>
        public event delConnecterInfo<TContract> BeforeConnect;

        /// <summary>
        /// Raising after client connection was succesfully done
        /// </summary>
        public event delConnecter<TContract> AfterConnect;

        /// <summary>
        /// Raising after client was disconnected
        /// </summary>
        public event delConnecter<TContract> OnDisconnect;

        /// <summary>
        /// Raising on server stop listening
        /// </summary>
        public event delListenStoppped OnListenStopped;

        /// <summary>
        /// Open server at specified ip port. Use ip = Any for opening at all known Network interfaces
        /// </summary>
        public void Open(System.Net.IPAddress ip, int port)
        {
            if (Server != null)
                throw new InvalidOperationException("Server is already open");
            contracts = new Dictionary<TContract, LightTunnelClient<TContract>>();
            Server = new LServer();
            Server.OnConnect += server_onClientConnect;
            Server.OnDisconnect += server_onClientDisconnect;
            Server.OnEnd += server_onEnd;
            Server.BeginListen(ip, port);

        }

        /// <summary>
        /// Close server, stop listening and disconnect all connected clients
        /// </summary>
        public void Close()
        {
            if (Server == null)
                throw new InvalidOperationException("Server is already closed");
            Server.StopServer();
            Server = null;
        }

        /// <summary>
        /// Return LightTunnelClient, associated with specified contract
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public LightTunnelClient<TContract> GetTunnel(TContract contract)
        {
            lock (contracts)
            {
                if (!contracts.ContainsKey(contract))
                    return null;
                else
                    return contracts[contract];
            }
        }

        /// <summary>
        /// Disconnect client associated with specified contract
        /// </summary>
        /// <param name="contract"></param>
        public void Kick(TContract contract)
        {
            var tunnel = GetTunnel(contract);
            tunnel.Disconnect();
        }

        #region private 

        private void server_onClientConnect(LServer sender, LClient newClient, ConnectInfo info)
        {
            var contract = new TContract();
            var tunnel = new LightTunnelClient<TContract>(newClient, contract);

            lock (contracts)
            {
                contracts.Add(contract, tunnel);
            }

            if (BeforeConnect != null)
                BeforeConnect(this, contract, info);
            if (!info.AllowConnection)
                return;

            newClient.AllowReceive = true;

            if (AfterConnect != null)
                AfterConnect(this, contract);
        }

        private void server_onClientDisconnect(LServer server, LClient oldClient)
        {
            TContract client = null;
            lock (contracts)
            {
                client = contracts.FirstOrDefault(c => c.Value.Client == oldClient).Key;
                if (client != null)
                    contracts.Remove(client);
                else
                    throw new Exception();
            }
            if (OnDisconnect != null)
                OnDisconnect(this, client);
        }

        private void server_onEnd(LServer arg1, Exception arg2)
        {
            if (OnListenStopped != null)
                OnListenStopped(this, arg2);
        }

        #endregion
    }

    public delegate void delListenStoppped(object sender, Exception sex);

    public delegate void delConnecterInfo<TContract>(
        LightTunnelServer<TContract> sender, TContract contract, ConnectInfo info) where TContract : class, new();

    public delegate void delConnecter<TContract>(LightTunnelServer<TContract> sender, TContract contract)
        where TContract : class, new();
}

