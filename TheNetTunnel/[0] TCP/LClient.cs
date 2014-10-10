using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;
using TheTunnel.Light;

namespace TheTunnel
{
	//based on http://robjdavey.wordpress.com/2011/02/11/asynchronous-tcp-client-example/ example
	public class LClient
	{	public static LClient Connect(IPAddress ip, int port)
		{	
			TcpClient client = new TcpClient ();

			client.Connect (new IPEndPoint (ip, port));

			if (client.Connected)
				return new LClient (client);
			else
				throw new System.Net.Sockets.SocketException();
		}

		public LClient (TcpClient client)
		{
			this.Client = client;
			sender = new QuantumSender ();
			receiver = new QuantumReceiver ();
			receiver.OnLightMessage += (QuantumReceiver arg1, QuantumHead arg2, System.IO.MemoryStream arg3) => {
				if(OnReceive!=null)
					OnReceive(this, arg3);
			};
		}

		public bool IsConnected{ get { return Client == null ? false : Client.Connected; } }

		public TcpClient Client{ get; protected set; }

		bool allowReceive = false;
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

		public event delQuantReceive OnReceive;

		public event Action<LClient> OnDisconnect;

		public void Stop(){
			if (Client.Connected)
				disconnect ();
		}

		public void SendMessage(MemoryStream streamOfLight)	{
			if (streamOfLight.Length == 20) {
			}
			lock(sender)
			{
				sender.Set (streamOfLight);
				byte[] buff;
				int id;
				while (sender.TryNext(maxQSize, out buff, out id)){
					write(buff);
				}
			}
		}

		#region private 
		int maxQSize = 900;

		QuantumSender sender;
		QuantumReceiver receiver;

		bool disconnectMsgWasSended = false;
		bool readWasStarted = false;

		void readCallback(IAsyncResult result)
		{
			int read;
			NetworkStream networkStream;
			try
			{
				networkStream = Client.GetStream();
				read = networkStream.EndRead(result);
			}
			catch
			{
				//An error has occured when reading
				disconnect ();
				return;
			}

			if (read == 0)
			{
				//The connection has been closed.
				disconnect ();
				return;
			}

			byte[] buffer = result.AsyncState as byte[];
			byte[] readed = new byte[read];
			Array.Copy (buffer, readed, read);


			if (!Client.Connected)
				return;

			receiver.Set (readed);

            
			//Start reading from the network again.
            try
            {
                networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
            }
            catch { return; }
		}

		/// <summary>
		/// Writes an array of bytes to the network.
		/// </summary>
		/// <param name="bytes">The array to write</param>
		/// <returns>A WaitHandle that can be used to detect
		/// when the write operation has completed.</returns>
		void write(byte[] bytes)
		{
			if (!Client.Connected)
				return;

			NetworkStream networkStream = Client.GetStream();

			//Start async write operation
			networkStream.BeginWrite(bytes, 0, bytes.Length, writeCallback, null);
		}

		/// <summary>
		/// Callback for Write operation
		/// </summary>
		/// <param name="result">The AsyncResult object</param>
		void writeCallback(IAsyncResult result)
		{
			try
			{
				NetworkStream networkStream = Client.GetStream();
				networkStream.EndWrite(result);
			}
			catch {
				disconnect ();
			}
		}

		void disconnect()
		{
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

