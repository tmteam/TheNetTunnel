using System.Net;
using TNT.Channel;

namespace Expirements.General
{
    public class TcpChannelServer<TContract> : ChannelServer<TContract, TcpChannel>
    {
        public IPEndPoint EndPoint { get; }

        public TcpChannelServer(
            TntContractBuilder<TContract, TcpChannel> channelBuilder, 
            IPEndPoint endPoint 
        ) : base(channelBuilder, new TcpChanelListener(endPoint))
        {
            EndPoint = endPoint;
        }
    }
}