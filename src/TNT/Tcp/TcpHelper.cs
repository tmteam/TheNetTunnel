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
      
        public static TcpChannelServer<TContract> CreateTcpServer<TContract>(this ConnectionBuilder<TContract> builder, IPAddress ip, int port) 
            where TContract : class
        {
            return new TcpChannelServer<TContract>(builder, new IPEndPoint(ip, port));
        }

        public static Connection<TContract, TcpChannel> CreateTcpClientConnection<TContract>(this ConnectionBuilder<TContract> builder, IPAddress ip, int port)
            where TContract : class

        {
            return builder.UseChannel(() =>
                    {
                        var channel = new TcpChannel();
                        channel.Connect(new IPEndPoint(IPAddress.Loopback, 17171));
                        return channel;
                    }).Buid();
        }
    }
}
