using System;
using System.Net;
using System.Net.Sockets;
namespace TheTunnel
{
//	(NO ONE IS - SAFE)
//	Noises, noises, people make noises
//	People make noises when they're sick
//	Nothing to do except hold on to NOTHING

	public class TcpClientTunnel{
		public TcpClientTunnel(){}
		public TcpClientTunnel(qTcpClient client, object contract){
			this.Client = client;
			CordDispatcher = new CordDispatcher (contract);
		}

		public bool IsConnected{
			get{return client == null ? false : client.Client == null ? false : client.Client.Connected; }
		}

		qTcpClient client;
		public qTcpClient Client{ 
			get{return client; }
			protected set{client = value; 
				if (client != null) {
					Client.OnDisconnect+= onTcpDisconnect;
					Client.OnReceive+= OnReceive;
				}
			} }

		CordDispatcher cordDispatcher;
		public CordDispatcher CordDispatcher{
			get{return cordDispatcher; } 
			protected set{
				cordDispatcher = value;
				if (cordDispatcher != null)
				{
					cordDispatcher.NeedSend += needSend;
					var dscnctable = CordDispatcher.Contract as IDisconnectable;
					if(dscnctable!=null)
						dscnctable.DisconnectMe+= handleDisconnectMe;
			}}
		}
			
		public void Connect(IPAddress ip, int port, object contract)
		{
			Client =  qTcpClient.Connect (ip, port);
			CordDispatcher = new CordDispatcher (contract);
		}

		public void Disconnect()
		{
			disconnectReason = DisconnectReason.UserWish;
			Client.Stop ();
		}

		public event delTcpClientDisconnect OnDisconnect; 

		void OnReceive (qTcpClient arg1, qMsg arg2)
		{
			CordDispatcher.Handle (arg2.body);
		}

		void needSend (CordDispatcher arg1, byte[] arg2)
		{
			Client.SendMessage (arg2);
		}

		void onTcpDisconnect (qTcpClient obj){
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
	public delegate void delTcpClientDisconnect (TcpClientTunnel sender, DisconnectReason reason);
}

