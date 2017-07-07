using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace TheTunnel
{
	public class LServer{
		public System.Net.Sockets.TcpListener Listener{ get; protected set; }
	
		List<LClient> clients = new List<LClient>();
		/// <summary>
		/// List of all currently connected clients
		/// </summary>
        public LClient[] Clients{get { lock (clients) {
					return clients.ToArray ();
				}}}
        /// <summary>
        /// Raising on new client connection
        /// </summary>
		public event delLightInitConnect OnConnect;
        /// <summary>
        /// Raising on client disconnection
        /// </summary>
		public event delLightConnect OnDisconnect;
        /// <summary>
        /// Raising on server falls. Actualy it informs that you should resstart server.
        /// </summary>
        public event Action<LServer, Exception> OnEnd;

        /// <summary>
        /// Open server and start listen at address:port
        /// </summary>
        /// <param name="address">Server ip (use ANY for all avaliable device)</param>
        /// <param name="port">Server listen-port</param>
		public void BeginListen(IPAddress address, int port){
			lock(listenLocker){
				IsListening = true;
				Listener = new TcpListener (address, port);
				Listener.Start ();
				Listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptSocketCallback), Listener);
			}
		}
        /// <summary>
        /// Stop listening
        /// </summary>
		public void EndListen(){
			lock (listenLocker) {
				IsListening = false;
				Listener.Stop ();
			}
		}
        /// <summary>
        /// Stop listening and disconnect all connected clients
        /// </summary>
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

        public bool IsListening { get; protected set; }
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
                } catch(Exception sex) {
                    if (OnEnd != null)
                        OnEnd(this, sex);
                    IsListening = false;
                }

                if (client != null){
                    //...Registrating the client
                    var qClient = new LClient(client);
                    addClient(qClient);
                    listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptSocketCallback), Listener);
                }
                else{
                    IsListening = false;
                    if (OnEnd != null)
                        OnEnd(this, null);
                }

			}
		}
        /// <summary>
        /// Registarte new client
        /// </summary>
        /// <param name="client"></param>
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
        /// <summary>
        /// Client disconnection handler
        /// </summary>
        /// <param name="obj"></param>
		void client_OnDisconnect (LClient obj){
			lock (clients) {
				clients.Remove (obj);
			}
            obj.OnDisconnect -= client_OnDisconnect;
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

