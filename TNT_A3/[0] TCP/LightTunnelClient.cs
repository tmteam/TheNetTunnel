using System;
using System.Net;
using System.Net.Sockets;
namespace TheTunnel
{
//	(NO ONE IS - SAFE)
//	Noises, noises, people make noises
//	People make noises when they're sick
//	Nothing to do except hold on to NOTHING

	public class LightTunnelClient{
		public LightTunnelClient(){}
		public LightTunnelClient(LightTcpClient client, object contract){
			this.Client = client;
			CordDispatcher = new CordDispatcher (contract);
		}

		public bool IsConnected{
			get{return client == null ? false : client.Client == null ? false : client.Client.Connected; }
		}

		LightTcpClient client;
		public LightTcpClient Client{ 
			get{return client; }
			protected set{client = value; 
				if (client != null) {
					Client.OnDisconnect+= onTcpDisconnect;
					Client.OnReceive+= client_OnReceive;
				}
			} }

		void client_OnReceive (LightTcpClient client, System.IO.MemoryStream msg)
		{
			CordDispatcher.Handle (msg);
		}

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

		void cordDispather_NeedSend (CordDispatcher sender, System.IO.MemoryStream streamOfLight)
		{
			Client.SendMessage (streamOfLight);
		}
			
		public void Connect(IPAddress ip, int port, object contract)
		{
			Client =  LightTcpClient.Connect (ip, port);
			CordDispatcher = new CordDispatcher (contract);
		}

		public void Disconnect()
		{
			disconnectReason = DisconnectReason.UserWish;
			Client.Stop ();
		}

		public event delTcpClientDisconnect OnDisconnect; 

		void onTcpDisconnect (LightTcpClient obj){
			if (OnDisconnect != null)
				OnDisconnect (this, disconnectReason);

			if (CordDispatcher != null)
				CordDispatcher.OnDisconnect (disconnectReason);
		}

		void handleDisconnectMe (IDisconnectable obj){
			disconnectReason = DisconnectReason.ContractWish;
			Client.Stop ();
		}

		DisconnectReason disconnectReason = DisconnectReason.ConnectionIsLost;

	}
	public delegate void delTcpClientDisconnect (LightTunnelClient sender, DisconnectReason reason);
}

