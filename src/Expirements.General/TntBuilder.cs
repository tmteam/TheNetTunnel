using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TNT.Channel;

namespace Expirements.General
{
    public static class TntBuilder
    {
        public static TntContractBuilder<TContract>
            CreateConnectionBuilder<TContract>()
        {
            throw new NotImplementedException();

        }
        public static Connection<TContract, TcpChannel> CreateTcpConnection<TContract>(IPAddress ip, int port)
        {
            throw  new NotImplementedException();
        }
        public static TntContractBuilder<T> UseContract<T>()
        {
            throw  new NotImplementedException();
        }

        public static TntContractBuilder<T> UseContract<T>(T contractImplementation)
        {
            throw  new NotImplementedException();
        }
    }

    public class TntContractBuilder<T>
    {
        public TntContractBuilder<T> UseTcpClient(EndPoint endPoint)
        {
            return this;
        }
        public ConnectionBuilder<T, TcpChannel> UseTcpClient(IPAddress ip, int  port)
        {
            throw  new NotImplementedException();
        }
        public ConnectionBuilder<T, TChannel> UseChannel<TChannel>(TChannel channel) where TChannel : IChannel
        {
            throw new NotImplementedException();
        }


    }
}
