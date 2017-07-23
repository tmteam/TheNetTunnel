using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TNT.Transport;

namespace TNT.Api
{
    public class ChannelServer<TContract, TChannel>: IChannelServer<TContract,TChannel> 
        where TChannel :  IChannel
        where TContract: class
    {
        private readonly PresentationBuilder<TContract> _connectionBuilder;
        protected readonly IChannelListener<TChannel> Listener;

        readonly ConcurrentDictionary<IChannel, Connection<TContract, TChannel>> _connections
            = new ConcurrentDictionary<IChannel, Connection<TContract, TChannel>>();

        public bool IsListening {
            get { return Listener.IsListening; }
            set { Listener.IsListening = value; }
        }

        public event Action<IChannelServer<TContract, TChannel>, BeforeConnectEventArgs<TContract, TChannel>> 
            BeforeConnect;
        public event Action<IChannelServer<TContract, TChannel>, Connection<TContract, TChannel>> 
            AfterConnect;
        public event Action<IChannelServer<TContract, TChannel>, Connection<TContract, TChannel>> 
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