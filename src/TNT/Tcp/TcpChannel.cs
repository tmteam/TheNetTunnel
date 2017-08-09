using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TNT.Exceptions.Local;
using TNT.Presentation;
using TNT.Transport;

namespace TNT.Tcp
{
    public class TcpChannel : IChannel
    {
        private bool _wasConnected = false;

        private bool allowReceive = false;
        /// <summary>
        /// Actualy bool type. 
        /// </summary>
        private int disconnectIsHandled = 0;
        private bool readWasStarted = false;
        private int _bytesReceived;
        private int _bytesSent;

        public TcpChannel(IPAddress address, int port) : this(new TcpClient(new IPEndPoint(address, port)))
        {

        }
        public TcpChannel(TcpClient client)
        {
            Client = client;
            _wasConnected = client.Connected;
            SetEndPoints();
        }
        public TcpChannel()
        {
            Client = new TcpClient();
        }

        public int BytesReceived => _bytesReceived;

        public int BytesSent => _bytesSent;

        public string RemoteEndpointName { get; private set; }
        public string LocalEndpointName { get; private set; }

        public bool IsConnected => Client != null && Client.Connected;

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
                            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
                        }
                    }
                    if (!value)
                    {
                        throw new InvalidOperationException("Recceiving cannot be stoped");
                    }
                }
            }
        }


        public TcpClient Client { get; }

        public event Action<object, byte[]> OnReceive;
        public event Action<object, ErrorMessage> OnDisconnect;

        public void Connect(IPEndPoint endPoint)
        {
            this.Client.Connect(endPoint);
            _wasConnected = IsConnected;
            AllowReceive = true;
            SetEndPoints();
        }
        public void DisconnectBecauseOf(ErrorMessage errorOrNull)
        {
            //Thread race state. 
            //AsyncWrite, AsyncRead and main disconnect reason are in concurrence

            //if(disconnectIsHandled==0) disconnectIsHandled = 1; 
            //else return;
            if (Interlocked.CompareExchange(ref disconnectIsHandled, 1, 0) == 1)
                return;

            allowReceive = false;

            if (Client.Connected)
            {
                try
                {
                    Client.Close();
                }
                catch { /* ignored*/ }
            }
            OnDisconnect?.Invoke(this, errorOrNull);
        }
        public void Disconnect()
        {
            DisconnectBecauseOf(null);
        }

        public  Task WriteAsync(byte[] data)
        {
            if (!_wasConnected)
                throw new ConnectionIsNotEstablishedYet("tcp channel was not connected yet");

            if (!Client.Connected)
                throw new ConnectionIsLostException("tcp channel is not connected");

            try
            {
                NetworkStream networkStream = Client.GetStream();

                //According to msdn, the WriteAsync call is thread-safe.
                //No need to use lock
                var ans = networkStream.WriteAsync(data, 0, data.Length);

                Interlocked.Add(ref _bytesSent, data.Length);
                return ans;
            }
            catch (Exception e)
            {
                Disconnect();
                throw new ConnectionIsLostException(innerException: e,
                     message: "Write operation was failed");
            }
        }
        /// <summary>
        /// Writes the data to underlying channel
        /// </summary>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="ArgumentNullException"></exception>
        public void Write(byte[] data, int offset, int length)
        {
            if (!_wasConnected)
                throw new ConnectionIsNotEstablishedYet("tcp channel was not connected yet");

            if (!Client.Connected)
                throw new ConnectionIsLostException("tcp channel is not connected");

            try
            {

                NetworkStream networkStream = Client.GetStream();
                //Start async write operation
                //According to msdn, the WriteAsync call is thread-safe.
                //No need to use lock
                networkStream.BeginWrite(data, offset, length, WriteCallback, null);
                
                Interlocked.Add(ref _bytesSent, length);
            }
            catch (Exception e)
            {
                Disconnect();
                throw new ConnectionIsLostException(innerException: e,
                     message: "Write operation was failed");
            }
        }

        readonly object _writeLocker = new object();

        private void WriteCallback(IAsyncResult result)
        {
            try
            {
                NetworkStream networkStream = Client.GetStream();
                networkStream.EndWrite(result);
            }
            catch
            {
                Disconnect();
            }
        }

        private void ReadCallback(IAsyncResult result)
        {
            try
            {
                var networkStream = Client.GetStream();
                var bytesToRead = networkStream.EndRead(result);

                if (bytesToRead == 0)
                {
                    Disconnect();
                    return;
                }

                var buffer = result.AsyncState as byte[];
                var readed = new byte[bytesToRead];
                Buffer.BlockCopy(buffer, 0, readed, 0, bytesToRead);

                unchecked
                {
                    _bytesReceived += bytesToRead;
                }

                OnReceive?.Invoke(this, readed);

                //Start reading from the network again.
                networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
            }
            catch
            {
                Disconnect();
            }
        }

        void SetEndPoints()
        {
            if (IsConnected)
            {
                RemoteEndpointName = Client.Client.RemoteEndPoint.ToString();
                LocalEndpointName = Client.Client.LocalEndPoint.ToString();
            }
        }
    }
}
