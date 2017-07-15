using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TNT.Channel;
using TNT.Cord.Deserializers;
using TNT.Cord.Serializers;

namespace Expirements.General
{
    public class ConnectionBuilder<TContract>
    {
        public ConnectionBuilder()
        {
            
        }
        public ConnectionBuilder(Func<IChannel, TContract> contractFactory)
        {

        }
        public ConnectionBuilder(Func<TContract> contractFactory)
        {

        }
        public ConnectionBuilder(TContract theContract)
        {

        }

        public  ContractContext<TContract, TcpChannel> CreateTcpConnection(IPAddress ip, int port)
        {
            throw new NotImplementedException();
        }
        public  TntContractBuilder<TContract, TChannel> For<TChannel>(TChannel channel)
        {
            throw new NotImplementedException();
        }
    }
    public static class TntBuilder
    {
        public static TntContractBuilder<TContract>
            CreateConnectionBuilder<TContract>()
        {
            throw new NotImplementedException();

        }
        public static ContractContext<TContract, TcpChannel> CreateTcpConnection<TContract>(IPAddress ip, int port)
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
    
    public class TntContractBuilder<TContract, TChannel>
    {
        public TntContractBuilder<TContract, TChannel> UseChannel(TChannel channel)
        {
            throw new NotImplementedException();
        }
        public ContractContext<TContract, TChannel> Buid()
        {
            throw new NotImplementedException();
        }
        public TContract BuidStateLess()
        {
            throw new NotImplementedException();
        }
        public TntContractBuilder<TContract, TChannel> UseSerializer(Predicate<Type> checker, ISerializer serializer)
        {
            return this;
        }
        public TntContractBuilder<TContract, TChannel> UseDeserializer(Predicate<Type> checker, IDeserializer serializer)
        {
            return this;
        }

        public TntContractBuilder<TContract, TChannel> UseSerializer<TType>(ISerializer serializer)
        {
            return this;

        }
        public TntContractBuilder<TContract, TChannel> UseDeserializer<TType>(IDeserializer serializer)
        {
            return this;
        }
    }

    public class TntContractBuilder<T>
    {
        public TntContractBuilder<T> UseTcpClient(EndPoint endPoint)
        {
            return this;
        }
        public TntContractBuilder<T, TcpChannel> UseTcpClient(IPAddress ip, int  port)
        {
            throw  new NotImplementedException();
        }
        public TntContractBuilder<T, TChannel> UseChannel<TChannel>(TChannel channel)
        {
            throw new NotImplementedException();
        }


    }

    public class ContractContext<TContract, TChannel>: IDisposable 
        //where TChannel: IChannel
    {
        public TContract Contract { get; }
        public TChannel Channel { get; }
        public void Dispose()
        {
        }
    }
}
