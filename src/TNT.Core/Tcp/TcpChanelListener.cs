using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TNT.Api;
using TNT.Presentation;

namespace TNT.Tcp
{
    public class TcpChanelListener : IChannelListener<TcpChannel>
    {
        private readonly IPEndPoint _endpoint;

        private TcpListener _listener = null;

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
                    _listener.Start();
                    var task = Listen();
                }
                else
                {
                    _listener.Stop();
                    _listener = null;
                }
            }
        }

        async Task Listen()
        {
            
            while (true)
            {
                var listener = _listener;
                TcpClient client = null;
                try
                {
                    if (_listener == null)
                        return;
                    client = await listener.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                if (_listener == null)
                    return;
                var channel = new TcpChannel(client);
                Accepted?.Invoke(this, channel);
            }
        }

        public event Action<IChannelListener<TcpChannel>, TcpChannel> Accepted;

    }
}