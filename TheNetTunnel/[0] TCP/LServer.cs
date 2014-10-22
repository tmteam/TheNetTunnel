using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;


namespace TheTunnel
{
	public class LServer{
		public System.Net.Sockets.TcpListener Listener{ get; protected set; }
	
		List<LClient> clients = new List<LClient>();
		public LClient[] Clients{get { lock (clients) {
					return clients.ToArray ();
				}}}

		public event delLightInitConnect OnConnect;
		public event delLightConnect OnDisconnect;

		public void BeginListen(IPAddress address, int port){
			lock(listenLocker){
				IsListening = true;
				Listener = new TcpListener (address, port);
				Listener.Start ();
				Listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptSocketCallback), Listener);
			}
		}

		public void EndListen(){
			lock (listenLocker) {
				IsListening = false;
				Listener.Stop ();
			}
		}

		public void StopServer(){
			lock(listenLocker){
				if (IsListening)
					EndListen ();
				var cl = Clients;
				foreach (var c in cl)
					c.Close ();
			}
		}


		#region private

		bool IsListening = false;
		object listenLocker = new object();
        /// <summary>
		/// Process the client connection.
		/// </summary>
		/// <param name="ar">Ar.</param>
		void DoAcceptSocketCallback(IAsyncResult ar){
			lock (listenLocker) {
				if (!IsListening)
					return;
				// Get the listener that handles the client request.
				TcpListener listener = (TcpListener)ar.AsyncState;
				// End the operation and...
                TcpClient client = null;
                
                try {
                    client = listener.EndAcceptTcpClient(ar);
                } catch { }

                if (client != null){
                    //...Registrating the client
                    var qClient = new LClient(client);
                    addClient(qClient);
                    //Connetion acception
                    listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptSocketCallback), Listener);
                }
			}
		}

		void addClient(LClient client){
			lock (clients) {
				clients.Add (client);
			}
			client.OnDisconnect+= client_OnDisconnect;
			ConnectInfo info = new ConnectInfo (client);
			if (OnConnect != null)
				OnConnect (this, client, info);

			if (!info.AllowConnection)	
				client.Close ();
		}

		void client_OnDisconnect (LClient obj){
			lock (clients) {
				clients.Remove (obj);
			}
			if (OnDisconnect != null)
				OnDisconnect (this, obj);
		}

		#endregion
	}
	public delegate void delLightConnect(LServer sender, LClient client);

	public delegate void delLightInitConnect(LServer sender, LClient client, ConnectInfo info);

	public class ConnectInfo{
		public ConnectInfo(LClient client)
		{
			this.Client = client;
		}
		public readonly LClient Client;
		public bool AllowConnection = true;
	}
}

