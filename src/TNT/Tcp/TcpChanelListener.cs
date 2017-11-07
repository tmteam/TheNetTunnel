using System;
using System.Net;
using System.Net.Sockets;
using TNT.Api;
using TNT.Presentation;

namespace TNT.Tcp
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
                    _listener.Start();
                    _listenResults = _listener.BeginAcceptTcpClient(EndAcceptTcpClient, _listener);
                }
                else
                {
                    //_listener.EndAcceptTcpClient(_listenResults);
                    _listener.Stop();
                    _listener = null;
                    _listenResults = null;
                }
            }
        }
        public event Action<IChannelListener<TcpChannel>, TcpChannel> Accepted;
        
        private void EndAcceptTcpClient(IAsyncResult state)
        {
            var listener = state.AsyncState as TcpListener;
            if(listener==null)
                return;
            bool needAccept = true;
            try
            {
                var client = listener.EndAcceptTcpClient(state);
                var channel = new TcpChannel(client);
                Accepted?.Invoke(this, channel);
            }
            catch (SocketException)
            {
                needAccept = true;
            }
            catch (ObjectDisposedException)
            {
                needAccept = false;
                return;
            }
            finally
            {
                if(needAccept)
                    listener.BeginAcceptTcpClient(EndAcceptTcpClient, listener);
            }
        }

     
    }
}