using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TNT.Presentation;

namespace TNT.Channel.Tcp
{
    public static class TcpHelper
    {
        public static Connection<TContract, TcpChannel> CreateTcpConnection<TContract>(this ConnectionBuilder<TContract> builder, IPAddress ip, int port)
            where TContract : class
        {
            var channel = new TcpChannel(new TcpClient(new IPEndPoint(ip, port)));
            var channelBuilder = builder.UseChannel(channel);
            return channelBuilder.Buid();
        }
        public static TcpChannelServer<TContract> CreateTcpServer<TContract>(this ConnectionBuilder<TContract> builder, IPAddress ip, int port) 
            where TContract : class
        {
            return new TcpChannelServer<TContract>(builder, new IPEndPoint(ip, port));
        }
    }
}
