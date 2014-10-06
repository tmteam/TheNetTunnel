using System;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace TheTunnel
{
	//based on http://robjdavey.wordpress.com/2011/02/11/asynchronous-tcp-client-example/ example
	public class LightTcpClient
	{	public static LightTcpClient Connect(IPAddress ip, int port)
		{	TcpClient client = new TcpClient ();

			client.Connect (new IPEndPoint (ip, port));

			if (client.Connected)
				return new LightTcpClient (client);
			else
				throw new System.Net.Sockets.SocketException();
		}

		int maxQSize = 900;
		public LightTcpClient (TcpClient client)
		{
			this.Client = client;
			sender = new QuantumSender ();
			//sender.MaxQuantSize = 1024;
			receiver = new QuantumReceiver ();
			receiver.OnLightMessage += (QuantumReceiver arg1, QuantumHead arg2, System.IO.MemoryStream arg3) => {
				if(onReceive!=null)
					onReceive(this, arg3);
			};
		}

		public TcpClient Client{ get; protected set; }

		event delQuantReceive onReceive;

		public event delQuantReceive OnReceive
		{
			add   { 
				onReceive+= value;
				if(!readWasStarted)
					readWasStarted = true;
				NetworkStream networkStream = Client.GetStream();
				byte[] buffer = new byte[Client.ReceiveBufferSize];

				//Now we are connected.
				//start async read operation.
				networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
			}
			remove{ onReceive-= value;}
		}

		public event Action<LightTcpClient> OnDisconnect;

		public void Stop()
		{
			if (Client.Connected) {
				disconnect ();
			}
			throw new InvalidOperationException ();
		}

		public void SendMessage(MemoryStream streamOfLight)
		{
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
			Array.Resize (ref buffer, read);
			receiver.Set (buffer);

			if (!Client.Connected)
				return;

			//Start reading from the network again.
			networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
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
	}

	public delegate void delQuantReceive(LightTcpClient client, MemoryStream msg);
}

