using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TNT.Channel;

namespace Expirements.General
{
    public class ChannelServer<TContract, TChannel>: IChannelServer<TChannel,TContract> where TChannel : IChannel
    {
        private readonly ConnectionBuilder<TContract> _connectionBuilder;
        private readonly IChannelListener<TChannel> _listener;

        readonly ConcurrentDictionary<IChannel, Connection<TContract, TChannel>> _connections
            = new ConcurrentDictionary<IChannel, Connection<TContract, TChannel>>();

        public bool IsListening {
            get { return _listener.IsListening; }
            set { _listener.IsListening = value; }
        }

        public event Action<IChannelServer<TChannel, TContract>, BeforeConnectEventArgs<TContract, TChannel>> 
            BeforeConnect;
        public event Action<IChannelServer<TChannel, TContract>, Connection<TContract, TChannel>> 
            AfterConnect;
        public event Action<IChannelServer<TChannel, TContract>, Connection<TContract, TChannel>> 
            Disconnected;

        public ChannelServer(ConnectionBuilder<TContract> channelBuilder, IChannelListener<TChannel> listener)
        {
            _connectionBuilder = channelBuilder;
            _listener = listener;
            _listener.Accepted += _listener_Accepted;
        }

        private void _listener_Accepted(IChannelListener<TChannel> sender, TChannel channel)
        {
            channel.OnDisconnect += Channel_OnDisconnect;

            if (!channel.IsConnected)
                return;
            var connection = _connectionBuilder.UseChannel(channel).Buid();

            var beforeConnectEventArgs = new BeforeConnectEventArgs<TContract, TChannel>(connection);

            BeforeConnect?.Invoke(this, beforeConnectEventArgs);
            if (!beforeConnectEventArgs.AllowConnection)
                return;

            channel.AllowReceive = true;
            _connections.TryAdd(channel, connection);
            AfterConnect?.Invoke(this, connection);
        }

        public IEnumerable<Connection<TContract, TChannel>> GetAllConnections() {
            return _connections.Values.ToArray();
        }

        private void Channel_OnDisconnect(IChannel obj)
        {
            Connection<TContract, TChannel> connection;
            _connections.TryRemove(obj, out connection);
            if (connection != null)
                Disconnected?.Invoke(this, connection);
        }

        public void Close()
        {
            this.IsListening = false;
            foreach (var allConnection in GetAllConnections())
            {
                allConnection.Channel.Disconnect();
            }
        }
    }
}