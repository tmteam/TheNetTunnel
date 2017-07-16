using System.Net;

namespace TNT.Channel.Tcp
{
    public class TcpChannelServer<TContract> : ChannelServer<TContract, TcpChannel>
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