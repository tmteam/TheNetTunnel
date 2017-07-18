using System.Net;
using TNT.Channel.Tcp;

namespace TNT.Presentation
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