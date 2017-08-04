using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TNT.Presentation;
using TNT.Transport;

namespace TNT.Api
{
    public class ChannelServer<TContract, TChannel>: IChannelServer<TContract,TChannel>, IDisposable
        where TChannel :  IChannel
        where TContract: class
    {
        private readonly PresentationBuilder<TContract> _connectionBuilder;
        protected readonly IChannelListener<TChannel> Listener;

        readonly ConcurrentDictionary<IChannel, IConnection<TContract, TChannel>> _connections
            = new ConcurrentDictionary<IChannel, IConnection<TContract, TChannel>>();

        public bool IsListening {
            get { return Listener.IsListening; }
            set { Listener.IsListening = value; }
        }

        public event Action<object, BeforeConnectEventArgs<TContract, TChannel>> 
            BeforeConnect;
        public event Action<object, IConnection<TContract, TChannel>> 
            AfterConnect;
        public event Action<object, ClientDisconnectEventArgs<TContract, TChannel>> 
            Disconnected;

        public ChannelServer(PresentationBuilder<TContract> channelBuilder, IChannelListener<TChannel> listener)
        {
            _connectionBuilder = channelBuilder;
            Listener = listener;
            Listener.Accepted += _listener_Accepted;
        }

        private void _listener_Accepted(IChannelListener<TChannel> sender, TChannel channel)
        {
            channel.OnDisconnect += Channel_OnDisconnect;

            if (!channel.IsConnected)
                return;
            var connection = _connectionBuilder.UseChannel(channel).Build();

            var beforeConnectEventArgs = new BeforeConnectEventArgs<TContract, TChannel>(connection);

            BeforeConnect?.Invoke(this, beforeConnectEventArgs);
            if (!beforeConnectEventArgs.AllowConnection)
                return;

            channel.AllowReceive = true;
            _connections.TryAdd(channel, connection);
            AfterConnect?.Invoke(this, connection);
        }

        public IEnumerable<IConnection<TContract, TChannel>> GetAllConnections() {
            return _connections.Values.ToArray();
        }

        private void Channel_OnDisconnect(object obj, ErrorMessage cause)
        {
            IConnection<TContract, TChannel> connection;
            _connections.TryRemove((IChannel)obj, out connection);
            if (connection != null)
                Disconnected?.Invoke(this, new ClientDisconnectEventArgs<TContract, TChannel>(connection, cause));
        }

        public void Close()
        {
            this.IsListening = false;
            foreach (var allConnection in GetAllConnections())
            {
                allConnection.Channel.Disconnect();
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}