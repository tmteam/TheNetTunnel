using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TheTunnel.Cords;


namespace TheTunnel
{
//	(NO ONE IS - SAFE)
//	Noises, noises, people make noises
//	People make noises when they're sick
//	Nothing to do except hold on to NOTHING


	public class LightTunnelClient<T> where T: class, new() {
		
        public LightTunnelClient(){
            this.Contract = new T();
        }

        public LightTunnelClient(T Contract){
            this.Contract = Contract;
        }

        public LightTunnelClient(LClient client, T contract){
            this.Contract = contract;
            CordDispatcher = new CordDispatcher<T> (contract);
			this.Client = client;

		}
        public T Contract { get; protected set; }
		public bool IsConnected{
			get{return client == null ? false : client.Client == null ? false : client.Client.Connected; }
		}

		LClient client;
		public LClient Client{ 
			get{return client; }
			protected set{
                if (client != null){
                    Client.OnDisconnect -= onTcpDisconnect;
                    Client.OnReceive -= client_OnReceive;
                }
                client = value; 
				if (client != null) {
					Client.OnDisconnect+= onTcpDisconnect;
					Client.OnReceive+= client_OnReceive;
				}
			} }

		CordDispatcher<T> cordDispatcher;
		public CordDispatcher<T> CordDispatcher{
			get{return cordDispatcher; } 
			protected set{
                if(cordDispatcher!=null){
                    cordDispatcher.NeedSend -= cordDispather_NeedSend;
                    var dscnctable = CordDispatcher.Contract as IDisconnectable;
					if(dscnctable!=null)
						dscnctable.DisconnectMe-= handleDisconnectMe;
                }
				cordDispatcher = value;
				if (cordDispatcher != null){
					cordDispatcher.NeedSend+= cordDispather_NeedSend;
					var dscnctable = CordDispatcher.Contract as IDisconnectable;
					if(dscnctable!=null)
						dscnctable.DisconnectMe+= handleDisconnectMe;
			}}
		}
			
		public event delTcpClientDisconnect OnDisconnect; 

		public void Connect(IPAddress ip, int port, T contract)
		{
            this.Contract = contract;
			CordDispatcher = new CordDispatcher<T> (contract);
			Client =  LClient.Connect (ip, port);
			Client.AllowReceive = true;
		}
        
        public void Connect(IPAddress ip, int port){
            if (Contract == default(T))
                Contract = new T();
            Connect(ip, port, Contract);
        }

		public void Disconnect()
		{
			disconnectReason = DisconnectReason.UserWish;
			Client.Close ();
		}

		DisconnectReason disconnectReason = DisconnectReason.ConnectionIsLost;

		#region private

		void client_OnReceive (LClient client, System.IO.MemoryStream msg)
		{
			ThreadPool.QueueUserWorkItem((s)=>CordDispatcher.Handle (msg));
		}

		void cordDispather_NeedSend (object sender, System.IO.MemoryStream streamOfLight)
		{
			Client.SendMessage (streamOfLight);
		}

		void onTcpDisconnect (LClient obj){
			if (OnDisconnect != null)
				OnDisconnect (this, disconnectReason);

			if (CordDispatcher != null)
				CordDispatcher.OnDisconnect (disconnectReason);
		}

		void handleDisconnectMe (IDisconnectable obj){
			disconnectReason = DisconnectReason.ByContract;
			Client.Close ();
		}

		#endregion
	}

	public delegate void delTcpClientDisconnect (object sender, DisconnectReason reason);
}

