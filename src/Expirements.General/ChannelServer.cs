using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TNT.Channel;

namespace Expirements.General
{
    public class ChannelServer<TContract, TChannel>: IChannelServer<TChannel,TContract> where TChannel : IChannel
    {
        private readonly TntContractBuilder<TContract, TChannel> _channelBuilder;
        private readonly IChannelListener<TChannel> _listener;

        readonly ConcurrentDictionary<IChannel, ContractContext<TContract, TChannel>> _connections
            = new ConcurrentDictionary<IChannel, ContractContext<TContract, TChannel>>();

        public bool IsListening {
            get { return _listener.IsListening; }
            set { _listener.IsListening = value; }
        }

        public event Action<IChannelServer<TChannel, TContract>, BeforeConnectEventArgs<TContract, TChannel>> BeforeConnect;
        public event Action<IChannelServer<TChannel, TContract>, ContractContext<TContract, TChannel>> AfterConnect;
        public event Action<IChannelServer<TChannel, TContract>, ContractContext<TContract, TChannel>> Disconnected;

        public ChannelServer(TntContractBuilder<TContract, TChannel> channelBuilder, IChannelListener<TChannel> listener)
        {
            _channelBuilder = channelBuilder;
            _listener = listener;
            _listener.Accepted += _listener_Accepted;
        }

        private void _listener_Accepted(IChannelListener<TChannel> sender, TChannel channel)
        {
            channel.OnDisconnect += Channel_OnDisconnect;
            if (!channel.IsConnected)
                return;
            var connection = _channelBuilder.UseChannel(channel).Buid();
            var beforeConnectEventArgs = new BeforeConnectEventArgs<TContract, TChannel>(connection);

            OnBeforeConnect(beforeConnectEventArgs);
            if (!beforeConnectEventArgs.AllowConnection)
                return;

            channel.AllowReceive = true;
            _connections.TryAdd(channel, connection);
            OnAfterConnect(connection);
        }

        public IEnumerable<ContractContext<TContract, TChannel>> GetAllConnections()
        {
            return _connections.Values.ToArray();
        }

        protected virtual void OnBeforeConnect(BeforeConnectEventArgs<TContract, TChannel> arg2)
        {
            BeforeConnect?.Invoke(this, arg2);
        }

        protected virtual void OnAfterConnect(ContractContext<TContract, TChannel> arg2)
        {
            AfterConnect?.Invoke(this, arg2);
        }

        protected virtual void OnDisconnected(ContractContext<TContract, TChannel> arg2)
        {
            Disconnected?.Invoke(this, arg2);
        }


        public void StartListen()
        {
            _listener.IsListening = true;
        }

        public void EndListen()
        {
            _listener.IsListening = false;
        }

        private void Channel_OnDisconnect(IChannel obj)
        {
            ContractContext<TContract, TChannel> connection;
            _connections.TryRemove(obj, out connection);
            if (connection != null)
                OnDisconnected(connection);
        }
    }
}