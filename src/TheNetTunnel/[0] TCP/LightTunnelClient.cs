using System;
using System.Net;
using System.Threading;
using TheTunnel.Cords;


namespace TheTunnel
{
    //	(NO ONE IS - SAFE)
    //	Noises, noises, people make noises
    //	People make noises when they're sick
    //	Nothing to do except hold on to NOTHING

    public class LightTunnelClient<T> where T : class, new()
    {

        public LightTunnelClient()
        {
            Contract = new T();
        }

        public LightTunnelClient(T Contract)
        {
            this.Contract = Contract;
        }

        public LightTunnelClient(LClient client, T contract)
        {
            Contract = contract;
            CordDispatcher = new CordDispatcher<T>(contract);
            Client = client;

        }

        /// <summary>
        /// Contract, associated with current client
        /// </summary>
        public T Contract { get; protected set; }

        /// <summary>
        /// Is client connected?
        /// </summary>
        public bool IsConnected
        {
            get { return client == null ? false : client.Client == null ? false : client.Client.Connected; }
        }

        private LClient client;

        /// <summary>
        /// Object of light - transport level client
        /// </summary>
        public LClient Client
        {
            get { return client; }
            protected set
            {
                if (client != null)
                {
                    Client.OnDisconnect -= onTcpDisconnect;
                    Client.OnReceive -= client_OnReceive;
                }
                client = value;
                if (client != null)
                {
                    Client.OnDisconnect += onTcpDisconnect;
                    Client.OnReceive += client_OnReceive;
                }
            }
        }

        private CordDispatcher<T> cordDispatcher;

        public CordDispatcher<T> CordDispatcher
        {
            get { return cordDispatcher; }
            protected set
            {
                if (cordDispatcher != null)
                {
                    cordDispatcher.NeedSend -= cordDispather_NeedSend;
                    var dscnctable = CordDispatcher.Contract as IDisconnectable;
                    if (dscnctable != null)
                        dscnctable.DisconnectMe -= handleDisconnectMe;
                }
                cordDispatcher = value;
                if (cordDispatcher != null)
                {
                    cordDispatcher.NeedSend += cordDispather_NeedSend;
                    var dscnctable = CordDispatcher.Contract as IDisconnectable;
                    if (dscnctable != null)
                        dscnctable.DisconnectMe += handleDisconnectMe;
                }
            }
        }

        public event delTcpClientDisconnect OnDisconnect;

        /// <summary>
        /// Connect to remote server at ip: port with specified contract
        /// </summary>
        public void Connect(IPAddress ip, int port, T contract)
        {
            Contract = contract;
            CordDispatcher = new CordDispatcher<T>(contract);
            Client = LClient.Connect(ip, port);
            Client.AllowReceive = true;
        }

        /// <summary>
        /// Connect to remote server at ip: port with default contract
        /// </summary>
        public void Connect(IPAddress ip, int port)
        {
            if (Contract == default(T))
                Contract = new T();
            Connect(ip, port, Contract);
        }

        /// <summary>
        /// Disconnect from remote Light server
        /// </summary>
        public void Disconnect()
        {
            disconnectReason = DisconnectReason.UserWish;
            Client.Close();
        }

        private DisconnectReason disconnectReason = DisconnectReason.ConnectionIsLost;

        #region private

        private void client_OnReceive(LClient client, System.IO.MemoryStream msg)
        {
            if (IsConnected)
                ThreadPool.QueueUserWorkItem((s) =>
                    {
                        try
                        {
                            CordDispatcher.Handle(msg);
                        }
                        catch (Exception)
                        {
                            if (IsConnected)
                            {
                                disconnectReason = DisconnectReason.ConnectionIsLost;
                                Client.Close();
                            }
                        }
                    }
                );
        }

        private void cordDispather_NeedSend(object sender, System.IO.MemoryStream streamOfLight)
        {
            if (IsConnected)
                Client.SendMessage(streamOfLight);
        }

        private void onTcpDisconnect(LClient obj)
        {
            if (OnDisconnect != null)
                OnDisconnect(this, disconnectReason);

            if (CordDispatcher != null)
                CordDispatcher.OnDisconnect(disconnectReason);
        }

        private void handleDisconnectMe(IDisconnectable obj)
        {
            disconnectReason = DisconnectReason.ByContract;
            Client.Close();
        }

        #endregion
    }

    public delegate void delTcpClientDisconnect(object sender, DisconnectReason reason);
}

