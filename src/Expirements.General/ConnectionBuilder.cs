using System;
using System.Net;
using System.Net.Sockets;
using TNT.Channel;
using TNT.Cord.Deserializers;
using TNT.Cord.Serializers;

namespace Expirements.General
{
    public class ConnectionBuilder<TContract, TChannel> where TChannel: IChannel
    {
        public Connection<TContract, TChannel> Buid()
        {
            throw new NotImplementedException();
        }
        public TContract BuidStateLess()
        {
            throw new NotImplementedException();
        }
    }
    //public class ConnectionBuilder<TContract, TContractImplementation> where TContractImplementation : new()
    //{
        
    //}

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
        public ConnectionBuilder<TContract> UseReceiveDispatcher<TDispatcher>() where TDispatcher : new()
        {
            return this;
        }
        public ConnectionBuilder<TContract> UseSerializer(Predicate<Type> checker, ISerializer serializer)
        {
            return this;
        }
        public ConnectionBuilder<TContract> UseDeserializer(Predicate<Type> checker, IDeserializer serializer)
        {
            return this;
        }

        public ConnectionBuilder<TContract> UseSerializer<TType>(ISerializer serializer)
        {
            return this;
        }
        public ConnectionBuilder<TContract> UseDeserializer<TType>(IDeserializer serializer)
        {
            return this;
        }

        public  Connection<TContract, TcpChannel> CreateTcpConnection(IPAddress ip, int port)
        {
            var channel = new TcpChannel(new TcpClient(new IPEndPoint(ip, port)));
            var builder =this.UseChannel(channel);
            return builder.Buid();
        }
        public TcpChannelServer<TContract> CreateTcpServer(IPAddress ip, int port)
        {
            return new TcpChannelServer<TContract>(this, new IPEndPoint(ip, port));
        }


        public ConnectionBuilder<TContract, TChannel> UseChannel<TChannel>() where TChannel: IChannel, new() 
        {
            throw new NotImplementedException();
        }
        
        public ConnectionBuilder<TContract, TChannel> UseChannel<TChannel>(TChannel theChannel) where TChannel : IChannel
        {
            throw new NotImplementedException();
        }
        public ConnectionBuilder<TContract, TChannel> UseChannel<TChannel>(Func<TChannel> theChannel) where TChannel : IChannel
        {
            throw new NotImplementedException();
        }

    }
}