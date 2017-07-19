using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TNT.Channel.Tcp
{
    public class TcpChannel : IChannel
    {
        private bool _wasConnected = false;

        private bool allowReceive = false;
        private bool disconnectMsgWasSended = false;
        private bool readWasStarted = false;

        public TcpChannel(IPAddress address, int port): this(new TcpClient(new IPEndPoint(address, port)))
        {
            
        }
        public TcpChannel(TcpClient client)
        {
            Client = client;
        }
        public TcpChannel()
        {
            Client = new TcpClient();
        }
        public bool IsConnected
        {
            get { return Client != null && Client.Connected; }
        }

        /// <summary>
        /// Can LClient-user can handle messages now?.
        /// </summary>
        public bool AllowReceive
        {
            get { return allowReceive; }
            set
            {
                if (allowReceive != value)
                {
                    allowReceive = value;
                    if (value)
                    {
                        if (!readWasStarted)
                        {
                            readWasStarted = true;
                            NetworkStream networkStream = Client.GetStream();
                            byte[] buffer = new byte[Client.ReceiveBufferSize];

                            //start async read operation.
                            //IOException
                            networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
                        }
                    }
                    if (!value)
                    {
                        throw new InvalidOperationException("Cannot stop reading");
                    }
                }
            }
        }


        public TcpClient Client { get; }

        public event Action<IChannel, byte[]> OnReceive;
        public event Action<IChannel> OnDisconnect;

        public void Connect(IPEndPoint endPoint)
        {
            this.Client.Connect(endPoint);
            _wasConnected = IsConnected;
            AllowReceive = true;
        }
        public void Disconnect()
        {
            if (Client.Connected)
                disconnect();
        }

        public async Task<bool> TryWriteAsync(byte[] array)
        {
            var stream = Client.GetStream();
            try
            {
                var task = stream.WriteAsync(array, 0, array.Length);
                await task;
                if (task.Exception == null)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

      public void Write(byte[] data)
        {
            if (!_wasConnected)
                throw new TNT.Exceptions.ConnectionIsNotEstablishedYet("tcp channel was not connected yet");

            if (!Client.Connected)
            {
                throw new TNT.Exceptions.ConnectionIsLostException("tcp channel is not connected");
            }

            try
            {
                NetworkStream networkStream = Client.GetStream();
                //Start async write operation
                networkStream.BeginWrite(data, 0, data.Length, writeCallback, null);

            }
            catch (Exception e)
            {
                if (!IsConnected)// e is IOException || e is InvalidOperationException)
                {
                    throw new TNT.Exceptions.ConnectionIsLostException(innerException: e,
                        message: "Write operation was failed");
                }
                else
                    throw;
            }
        }

        private void writeCallback(IAsyncResult result)
        {
            try
            {
                NetworkStream networkStream = Client.GetStream();
                networkStream.EndWrite(result);
            }
            catch
            {
                disconnect();
            }
        }

        private void readCallback(IAsyncResult result)
        {
            try
            {
                var networkStream = Client.GetStream();
                var bytesToRead = networkStream.EndRead(result);

                if (bytesToRead == 0)
                {
                    //The connection has been closed.
                    throw new Exception("Read of 0. Connection was closed");
                }

                var buffer = result.AsyncState as byte[];
                var readed = new byte[bytesToRead];
                //Array.Copy(buffer, readed, read);
                Buffer.BlockCopy(buffer, 0, readed, 0, bytesToRead);

                OnReceive?.Invoke(this, readed);

                //Start reading from the network again.
                networkStream.BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
            }
            catch
            {
                disconnect();
            }
        }
        
        private void disconnect()
        {
            if (Client.Connected)
            {    try
                {
                    Client.Close();
                }
                catch
                {
                    // ignored
                }
            }

            if (!disconnectMsgWasSended)
            {
                disconnectMsgWasSended = true;

                OnDisconnect?.Invoke(this);
            }
        }
    }
}
