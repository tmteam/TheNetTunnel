using System;
using System.Net;
using System.Threading;
using TNT;
using TNT.Exceptions.Local;
using TNT.Tcp;

namespace EX_2.Stage2_ComplexExample
{
    public class Server
    {
        private int messageId = 0;
        private readonly TcpChannelServer<IStage2Contract> _tntServer;
        public Server()
        {
            _tntServer = TntBuilder
                    //Using contract factory. ServerSideContractImplementation exemplar needs reference to broadcast method:
                    .UseContract<IStage2Contract>(() => new Stage2ContractImplementation(this))
                    .CreateTcpServer(IPAddress.Any, 12345);

            //alow only 10 connections at the moment:
            _tntServer.BeforeConnect += (_, arg) =>
            {
                if (_tntServer.ConnectionsCount >= 10)
                    arg.AllowConnection = false;
            };
            _tntServer.StartListening();
            Console.WriteLine("Server opened");

        }


        public int SendBroadCast(DateTime timeStamp, string user, string message)
        {
            var id = Interlocked.Increment(ref messageId);
            var connections = _tntServer.GetAllConnections();
            //Send broadcast to all clients:
            foreach (var connection in connections)
            {
                try
                {
                    connection.Contract.NewMessageReceived(new ChatMessage
                    {
                        Message = message,
                        MessageId = id,
                        Timestamp = timeStamp,
                        User = user
                    });
                }
                catch (TntCallException e)
                {
                    //handle disconnect here
                }
            }
            return id;
        }

        public void Dispose()
        {
            _tntServer.Dispose();
        }
    }
}
