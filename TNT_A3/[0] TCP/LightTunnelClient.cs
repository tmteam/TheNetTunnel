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

	public class LightTunnelClient{
		public LightTunnelClient(){}
		public LightTunnelClient(LClient client, object contract){
			CordDispatcher = new CordDispatcher (contract);
			this.Client = client;
		}

		public bool IsConnected{
			get{return client == null ? false : client.Client == null ? false : client.Client.Connected; }
		}

		LClient client;
		public LClient Client{ 
			get{return client; }
			protected set{client = value; 
				if (client != null) {
					Client.OnDisconnect+= onTcpDisconnect;
					Client.OnReceive+= client_OnReceive;
				}
			} }

		CordDispatcher cordDispatcher;
		public CordDispatcher CordDispatcher{
			get{return cordDispatcher; } 
			protected set{
				cordDispatcher = value;
				if (cordDispatcher != null)
				{
					cordDispatcher.NeedSend+= cordDispather_NeedSend;
					var dscnctable = CordDispatcher.Contract as IDisconnectable;
					if(dscnctable!=null)
						dscnctable.DisconnectMe+= handleDisconnectMe;
			}}
		}
			
		public event delTcpClientDisconnect OnDisconnect; 

		public void Connect(IPAddress ip, int port, object contract)
		{
			CordDispatcher = new CordDispatcher (contract);
			Client =  LClient.Connect (ip, port);
			Client.AllowReceive = true;
		}

		public void Disconnect()
		{
			disconnectReason = DisconnectReason.UserWish;
			Client.Stop ();
		}

		DisconnectReason disconnectReason = DisconnectReason.ConnectionIsLost;


		#region private

		void client_OnReceive (LClient client, System.IO.MemoryStream msg)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback((s)=>CordDispatcher.Handle (msg)));
		}

		void cordDispather_NeedSend (CordDispatcher sender, System.IO.MemoryStream streamOfLight)
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
			disconnectReason = DisconnectReason.ContractWish;
			Client.Stop ();
		}

		#endregion
	}

	public delegate void delTcpClientDisconnect (LightTunnelClient sender, DisconnectReason reason);
}

