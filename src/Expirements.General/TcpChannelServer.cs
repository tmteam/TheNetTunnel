using System.Net;
using TNT.Channel;

namespace Expirements.General
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