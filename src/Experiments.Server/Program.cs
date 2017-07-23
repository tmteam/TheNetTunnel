using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Expirements.General;
using TNT.Api;
using TNT.Presentation;
using TNT.Tcp;

namespace Experiments.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var tcpServer = TntBuilder
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
