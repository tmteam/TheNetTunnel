using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Expirements.General;
using TNT.Channel;
using TNT.Channel.Tcp;
using TNT.Cord;
using TNT.Cord.Deserializers;
using TNT.Cord.Serializers;
using TNT.Light;
using TNT.Light.Sending;
using TNT.Presentation;
using TNT.Presentation.Origin;
using TNT.Presentation.Proxy;

namespace Experiments.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var tcpServer = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .CreateTcpServer(IPAddress.Loopback, 17171);
            tcpServer.BeforeConnect+= TcpServerOnBeforeConnect;
            tcpServer.AfterConnect += TcpServer_AfterConnect;
            tcpServer.Disconnected += TcpServer_Disconnected;
            Console.WriteLine("Start listen");
            tcpServer.IsListening = true;
            Console.WriteLine("Listen started");

            while (true)
            {
                var line = Console.ReadLine();
                if(line.ToLower()=="exit")
                    break;
                foreach (var connection in tcpServer.GetAllConnections())
                {
                   var returned = connection.Contract.AskCallBack(line);
                   Console.WriteLine($"Returned: {returned}");
                }
            }
            tcpServer.Close();
            Console.ReadLine();
        }

        private static void TcpServer_Disconnected(IChannelServer<ITestContract, TcpChannel> arg1, Connection<ITestContract, TcpChannel> arg2)
        {
            Console.WriteLine("Disconnected");
        }

        private static void TcpServer_AfterConnect(IChannelServer<ITestContract, TcpChannel> arg1, Connection<ITestContract, TcpChannel> arg2)
        {
            Console.WriteLine("Client connected");
        }

        private static void TcpServerOnBeforeConnect(IChannelServer<ITestContract, TcpChannel> channelServer, BeforeConnectEventArgs<ITestContract, TcpChannel> beforeConnectEventArgs)
        {
            Console.WriteLine("Before client connected");
        }

        
    }
}
