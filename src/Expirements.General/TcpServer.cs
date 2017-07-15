using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TNT.Channel;

namespace Expirements.General
{
    public class TcpServer<TContract>:IServer<TcpChannel,TContract>
    {
        private readonly TntContractBuilder<TContract, TcpChannel> _channelBuilder;
        private readonly IPEndPoint _endpoint;
        private TcpListener _listener = null;

        readonly ConcurrentDictionary<IChannel, ContractContext<TContract, TcpChannel>> _connections 
            = new ConcurrentDictionary<IChannel, ContractContext<TContract, TcpChannel>>();

        private IAsyncResult _listenResults;
        public bool IsListening => _listener != null;

        public event Action<IServer<TcpChannel, TContract>, BeforeConnectEventArgs<TContract, TcpChannel>> BeforeConnect;
        public event Action<IServer<TcpChannel, TContract>, ContractContext<TContract, TcpChannel>> AfterConnect;
        public event Action<IServer<TcpChannel, TContract>, ContractContext<TContract, TcpChannel>> Disconnected;
        public TcpServer(TntContractBuilder<TContract, TcpChannel> channelBuilder, IPEndPoint endpoint)
        {
            _channelBuilder = channelBuilder;
            _endpoint = endpoint;
        }
       
        public void StartListen()
        {
            _listener = new TcpListener(_endpoint);
            _listenResults = _listener.BeginAcceptTcpClient(EndAcceptTcpClient, _listener);
        }

        private void EndAcceptTcpClient(IAsyncResult state)
        {
            var listener = (TcpListener)state.AsyncState;
            try
            {
                var client = listener.EndAcceptTcpClient(state);
                var channel = new TcpChannel(client);
                channel.OnDisconnect += Channel_OnDisconnect;
                if (!channel.IsConnected)
                    return;
                var connection = _channelBuilder.UseChannel(channel).Buid();
                var beforeConnectEventArgs = new BeforeConnectEventArgs<TContract,TcpChannel>(connection);

                OnBeforeConnect(beforeConnectEventArgs);
                if (!beforeConnectEventArgs.AllowConnection)
                    return;

                channel.AllowReceive = true;
                _connections.TryAdd(channel, connection);
                OnAfterConnect(connection);
            }
            finally
            {
                listener.BeginAcceptTcpClient(EndAcceptTcpClient, listener);
            }
        }

        private void Channel_OnDisconnect(IChannel obj)
        {
            ContractContext<TContract, TcpChannel> connection;
            _connections.TryRemove(obj, out connection);
            if(connection!=null)
               OnDisconnected(connection);
        }

        public void EndListen()
        {
            _listener.EndAcceptTcpClient(_listenResults);
            _listener = null;
            _listenResults = null;
        }

        public IEnumerable<ContractContext<TContract, TcpChannel>> GetAllConnections()
        {
            return _connections.Values.ToArray();
        }


        protected virtual void OnBeforeConnect(BeforeConnectEventArgs<TContract, TcpChannel> arg2)
        {
            BeforeConnect?.Invoke(this, arg2);
        }

        protected virtual void OnAfterConnect(ContractContext<TContract, TcpChannel> arg2)
        {
            AfterConnect?.Invoke(this, arg2);
        }

        protected virtual void OnDisconnected(ContractContext<TContract, TcpChannel> arg2)
        {
            Disconnected?.Invoke(this, arg2);
        }
    }
}
