using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;
using TheTunnel.Light;

namespace TheTunnel
{
	//based on http://robjdavey.wordpress.com/2011/02/11/asynchronous-tcp-client-example/ example
	
    /// <summary>
    /// Light transport client
    /// </summary>
    public class LClient
	{	
        /// <summary>
        /// Create Light client connected to specified ip:port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static LClient Connect(IPAddress ip, int port){	
			var client = new TcpClient ();
        	client.Connect (new IPEndPoint (ip, port));

			if (client.Connected)
				return new LClient (client);
			else
				throw new System.Net.Sockets.SocketException();
		}
        /// <summary>
        /// Create Light client with specified TcpClient
        /// </summary>
        /// <param name="client"></param>
		public LClient (TcpClient client)
		{
			this.Client = client;
			qSender = new QuantumSender ();
			qReceiver = new QuantumReceiver ();
			qReceiver.OnLightMessage += (QuantumReceiver arg1, QuantumHead arg2, System.IO.MemoryStream arg3) => {
				if(OnReceive!=null)
					OnReceive(this, arg3);
			};
		}
        /// <summary>
        /// Indicates connection status of downlayer TcpClient
        /// </summary>
		public bool IsConnected{ get { return Client == null ? false : Client.Connected; } }
        /// <summary>
        /// Downlayer Tcp -connectionn
        /// </summary>
		public TcpClient Client{ get; protected set; }
		/// <summary>
        /// Can LClient-user can handle messages now?.
        /// </summary>
		public bool AllowReceive{
			get{ return allowReceive; }
			set { 
				if (allowReceive != value) {
					allowReceive = value; 
					if (value) {
						if (!readWasStarted) {
							readWasStarted = true;
							NetworkStream networkStream = Client.GetStream();
							byte[] buffer = new byte[Client.ReceiveBufferSize];
							//start async read operation.
							networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
						}
					}
					if (!value)
						throw new InvalidOperationException ("cannot stop reading. [spb]");
				}
			}
		}
        bool allowReceive = false;
        /// <summary>
        /// Raising on new light-message received.
        /// It is blocking operation (LClient cannot handle other messages, while OnReceive handling)
        /// </summary>
		public event delQuantReceive OnReceive;
        /// <summary>
        /// Raising if tcp connection is lost
        /// </summary>
		public event Action<LClient> OnDisconnect;
        /// <summary>
        /// Close tcp connection
        /// </summary>
		public void Close(){
			if (Client.Connected)
				disconnect ();
		}
        /// <summary>
        /// Send data that begins from stream position
        /// </summary>
        /// <param name="streamOfLight"></param>
		public void SendMessage(MemoryStream streamOfLight)	{
				qSender.Set (streamOfLight);
				byte[] buff;
				int id;
				while (qSender.TryNext(maxQSize, out buff, out id))
					write(buff);
		}

		#region private 

		int maxQSize = 900;
		QuantumSender qSender;
		QuantumReceiver qReceiver;
		bool disconnectMsgWasSended = false;
		bool readWasStarted = false;
        
		void readCallback(IAsyncResult result){
			try {
                var networkStream = Client.GetStream();
			    var read = networkStream.EndRead(result);
			    
                if (read == 0)
				    //The connection has been closed.
                    throw new Exception();
			    
                var buffer = result.AsyncState as byte[];
                //mb marshal??
			    var readed = new byte[read];
			    Array.Copy (buffer, readed, read);

                qReceiver.Set (readed);
			    //Start reading from the network again.
                networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
             } catch { disconnect(); }
		}

		// Writes an array of bytes to the network.
		void write(byte[] bytes) {
			if (!Client.Connected)
				return;

			NetworkStream networkStream = Client.GetStream();

			//Start async write operation
			networkStream.BeginWrite(bytes, 0, bytes.Length, writeCallback, null);
		}

		void writeCallback(IAsyncResult result){
			try{
                NetworkStream networkStream = Client.GetStream();
				networkStream.EndWrite(result);
			}catch {
				disconnect ();
			}
		}

		void disconnect(){
			if (Client.Connected)
				Client.Close ();
			if (!disconnectMsgWasSended) {
				disconnectMsgWasSended = true;
				if (OnDisconnect != null)
					OnDisconnect (this);
			}
		}
		#endregion
	}

	public delegate void delQuantReceive(LClient client, MemoryStream msg);
}

