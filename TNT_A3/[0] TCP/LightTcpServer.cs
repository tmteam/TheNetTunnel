using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;


namespace TheTunnel
{
	public class LightTcpServer
	{
		public System.Net.Sockets.TcpListener Listener{ get; protected set; }
		public LightTcpServer ()
		{
			MaxConnectionCount = 10000;
		}

		public int MaxConnectionCount{ get; set;}

		public void BeginListen(IPAddress address, int port)
		{
			lock(listenLocker)
			{
				IsListening = true;
				Listener = new TcpListener (address, port);
				Listener.Start ();
				Listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptSocketCallback), Listener);
			}
		}
		object listenLocker = new object();
		// Process the client connection.
		public void DoAcceptSocketCallback(IAsyncResult ar) 
		{
			lock (listenLocker) {
				if (!IsListening)
					return;

				// Get the listener that handles the client request.
				TcpListener listener = (TcpListener)ar.AsyncState;

				// End the operation and
				TcpClient client = listener.EndAcceptTcpClient (ar);
				if (client != null) {

					//Registrating the client
					var qClient = new LightTcpClient (client);
					addClient (qClient);
					//Connetining acception
					listener.BeginAcceptTcpClient (new AsyncCallback (DoAcceptSocketCallback), Listener);
				}
			}
		}

		public void EndListen()
		{
			lock (listenLocker) {
				IsListening = false;
				Listener.Stop ();
			}
		}

		public void StopServer()
		{
			lock(listenLocker)
			{
				if (IsListening)
					EndListen ();
				var cl = Clients;
				foreach (var c in cl)
					c.Stop ();
			}
		}

		bool IsListening = false;

		List<LightTcpClient> clients = new List<LightTcpClient>();
		public LightTcpClient[] Clients{get{lock (clients) {
					return clients.ToArray ();
				}}}

		public event delLightInitConnect OnConnect;
		public event delLightConnect OnDisconnect;

		void addClient(LightTcpClient client)
		{
			lock (clients) {
				clients.Add (client);
			}
			client.OnDisconnect+= client_OnDisconnect;
			ConnectInfo info = new ConnectInfo (client);
			if (OnConnect != null)
				OnConnect (this, client, info);

			if (!info.AllowConnection)	
				client.Stop ();
		}

		void client_OnDisconnect (LightTcpClient obj)
		{
			lock (clients) {
				clients.Remove (obj);
			}
			if (OnDisconnect != null)
				OnDisconnect (this, obj);
		}
	}
	public delegate void delLightConnect(LightTcpServer sender, LightTcpClient client);

	public delegate void delLightInitConnect(LightTcpServer sender, LightTcpClient client, ConnectInfo info);
	public class ConnectInfo
	{
		public ConnectInfo(LightTcpClient client)
		{
			this.Client = client;
		}
		public readonly LightTcpClient Client;
		public bool AllowConnection = true;
	}
}

