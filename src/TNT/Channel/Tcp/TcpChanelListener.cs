using System;
using System.Net;
using System.Net.Sockets;

namespace TNT.Channel.Tcp
{
    public class TcpChanelListener : IChannelListener<TcpChannel>
    {
        private readonly IPEndPoint _endpoint;

        private TcpListener _listener = null;
        private IAsyncResult _listenResults = null;

        public TcpChanelListener(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
        }

        public bool IsListening
        {
            get { return _listener!= null; }
            set
            {
                if(IsListening == value)
                    return;
                if (value)
                {
                    _listener = new TcpListener(_endpoint);
                    _listenResults = _listener.BeginAcceptTcpClient(EndAcceptTcpClient, _listener);
                }
                else
                {
                    _listener.EndAcceptTcpClient(_listenResults);
                    _listener = null;
                    _listenResults = null;
                }
            }
        }
        public event Action<IChannelListener<TcpChannel>, TcpChannel> Accepted;


        private void EndAcceptTcpClient(IAsyncResult state)
        {
            var listener = (TcpListener)state.AsyncState;
            try
            {
                var client = listener.EndAcceptTcpClient(state);
                var channel = new TcpChannel(client);
                Accepted?.Invoke(this, channel);
            }
            finally
            {
                listener.BeginAcceptTcpClient(EndAcceptTcpClient, listener);
            }
        }
    }
}