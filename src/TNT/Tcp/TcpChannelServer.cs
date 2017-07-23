using System.Net;
using TNT.Api;
using TNT.Presentation;

namespace TNT.Tcp
{
    public class TcpChannelServer<TContract> : ChannelServer<TContract, TcpChannel> 
        where TContract : class
    {
        public IPEndPoint EndPoint { get; }

        public TcpChannelServer(
            ConnectionBuilder<TContract> connectionBuilder, 
            IPEndPoint endPoint 
        ) : base(connectionBuilder, new TcpChanelListener(endPoint))
        {
            EndPoint = endPoint;
        }
    }
}