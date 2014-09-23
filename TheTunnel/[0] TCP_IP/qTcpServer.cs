using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace TheTunnel
{
	public class qTcpServer
	{
		public System.Net.Sockets.TcpListener Listener{ get; protected set; }
		public qTcpServer ()
		{
			MaxConnectionCount = 10000;
		}

		public int MaxConnectionCount{ get; set;}

		public void BeginListen(IPAddress address, int port)
		{
			IsListening = true;
			Listener = new TcpListener (address, port);
			Listener.Start ();
			Listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptSocketCallback), Listener);
		}

		// Process the client connection.
		public void DoAcceptSocketCallback(IAsyncResult ar) 
		{
			if (!IsListening)
				return ;
			// Get the listener that handles the client request.
			TcpListener listener = (TcpListener) ar.AsyncState;
			// End the operation and
			TcpClient client = listener.EndAcceptTcpClient(ar);
			if (client != null) {
				//Registrating the client
				var qClient = new qTcpClient (client);
				addClient (qClient);
				//Connetining acception
				listener.BeginAcceptTcpClient (new AsyncCallback (DoAcceptSocketCallback), Listener);
			}
		}


		public void EndListen()
		{
			IsListening = false;
			Listener.Stop ();
		}

		public void StopServer()
		{
			if (IsListening)
				EndListen ();
			while (clients.Count > 0)
				clients.First ().Stop ();
		}

		bool IsListening = false;

		List<qTcpClient> clients = new List<qTcpClient>();
		public qTcpClient[] Clients{get{lock (clients) {
					return clients.ToArray ();
				} }}

		public event Action<qTcpServer,qTcpClient> OnConnect;
		public event Action<qTcpServer,qTcpClient> OnDisconnect;

		void addClient(qTcpClient client)
		{
			lock (clients) {
				clients.Add (client);
			}
			client.OnDisconnect+= client_OnDisconnect;
			if (OnConnect != null)
				OnConnect (this, client);
		}

		void client_OnDisconnect (qTcpClient obj)
		{
			lock (clients) {
				clients.Remove (obj);
			}
			if (OnDisconnect != null)
				OnDisconnect (this, obj);
		}

	}
}

