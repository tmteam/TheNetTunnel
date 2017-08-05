using System.Net;
using TNT.Api;
using TNT.Presentation;

namespace TNT.Tcp
{
    public static class TcpHelper
    {
      
        public static TcpChannelServer<TContract> CreateTcpServer<TContract>(this PresentationBuilder<TContract> builder, IPAddress ip, int port) 
            where TContract : class
        {
            return new TcpChannelServer<TContract>(builder, new IPEndPoint(ip, port));
        }
        public static IConnection<TContract, TcpChannel> CreateTcpClientConnection<TContract>(
         this PresentationBuilder<TContract> builder, IPEndPoint endPoint)
         where TContract : class

        {
            return builder.UseChannel(() =>
            {
                var channel = new TcpChannel();
                channel.Connect(endPoint);
                return channel;
            }).Build();
        }
        public static IConnection<TContract, TcpChannel> CreateTcpClientConnection<TContract>(
            this PresentationBuilder<TContract> builder, IPAddress ip, int port)
            where TContract : class

        {
            return CreateTcpClientConnection(builder, new IPEndPoint(ip, port));
        }
    }
}
