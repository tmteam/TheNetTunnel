using System;
using System.Net;
namespace TheTunnel
{
	public class TcpClientTunnel
	{
		public TcpClientTunnel(){}
		public TcpClientTunnel(qTcpClient client, object contract)
		{
			this.Client = client;
			CordDispatcher = new CordDispatcher2 (contract);
		}

		qTcpClient client;
		public qTcpClient Client{ 
			get{return client; }
			protected set{client = value; 
				if (client != null) {
					Client.OnDisconnect+= OnDisconnect;
					Client.OnReceive+= OnReceive;
				}
			} }

		CordDispatcher2 cordDispatcher;
		public CordDispatcher2 CordDispatcher{
			get{return cordDispatcher; } 
			protected set{
				cordDispatcher = value;
				if (cordDispatcher != null)
					cordDispatcher.NeedSend += needSend;
			}}

		public void Connect(IPAddress ip, int port, object contract)
		{
			Client =  qTcpClient.Connect (ip, port);
			Client.OnDisconnect+= OnDisconnect;
			Client.OnReceive+= OnReceive;
			CordDispatcher = new CordDispatcher2 (contract);
		}

		public void Disconnect()
		{
			if (CordDispatcher != null) {
				CordDispatcher.OnDisconnect ();
			}
			Client.Stop ();
		}
		void OnReceive (qTcpClient arg1, qMsg arg2)
		{
			CordDispatcher.Handle (arg2.body);
		}

		void needSend (CordDispatcher2 arg1, byte[] arg2)
		{
			Client.SendMessage (arg2);
		}

		void OnDisconnect (qTcpClient obj)
		{
			if (CordDispatcher != null) {
				CordDispatcher.OnDisconnect ();
			}
		}
	}
}

