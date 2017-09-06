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

        public TcpChannel(IPAddress address, int port)
        {
            Client = new TcpClient();
            Client.ConnectAsync(address, port).Wait();
            _wasConnected = Client.Connected;
            SetEndPoints();
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
                            var recTask = Receiving(networkStream);
                        }
                    }
                    if (!value)
                    {
                        throw new InvalidOperationException("Recceiving cannot be stoped");
                    }
                }
            }
        }

        async Task Receiving(NetworkStream stream)
        {
            byte[] buffer = new byte[Client.ReceiveBufferSize];
            try
            {
                while (true)
                {
                    var bytesToRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesToRead == 0)
                    {
                        Disconnect();
                        return;
                    }
                    var readed = new byte[bytesToRead];
                    Buffer.BlockCopy(buffer, 0, readed, 0, bytesToRead);

                    unchecked
                    {
                        _bytesReceived += bytesToRead;
                    }
                    OnReceive?.Invoke(this, readed);
                }
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }

        public TcpClient Client { get; }

        public event Action<object, byte[]> OnReceive;
        public event Action<object, ErrorMessage> OnDisconnect;

        public void Connect(IPEndPoint endPoint)
        {
            this.Client.ConnectAsync(endPoint.Address, endPoint.Port).Wait();
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
        object locker = new object();
        public Task WriteAsync(byte[] data)
        {
            if (!_wasConnected)
                throw new ConnectionIsNotEstablishedYet("tcp channel was not connected yet");

            if (!Client.Connected)
                throw new ConnectionIsLostException("tcp channel is not connected");

            try
            {
                lock (locker)
                {
                    NetworkStream networkStream = Client.GetStream();

                    //According to msdn, the WriteAsync call is thread-safe.
                    //No need to use lock
                    var ans = networkStream.WriteAsync(data, 0, data.Length);

                    Interlocked.Add(ref _bytesSent, data.Length);
                    return ans;
                }
            }
            catch (Exception e)
            {
                Disconnect();
                throw new ConnectionIsLostException(innerException: e,
                     message: "Write operation was failed");
            }
        }
        object writeLocker = new object();
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
                Task writeTask = null;
                lock (writeLocker)
                {
                    writeTask = networkStream.WriteAsync(data, offset, length);
                    writeTask.Wait();
                }
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
