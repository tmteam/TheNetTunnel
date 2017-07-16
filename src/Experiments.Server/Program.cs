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
        static TcpListener listener;
        static LightChannel channel;
        static CordMessenger messenger;

        static void Main(string[] args)
        {

            listener = new TcpListener(17171);
            Console.WriteLine("Start listen");

            listener.Start();
            Console.WriteLine("Listen started");

            var client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected");
            var contract = Factory.CreateForServer(client);

            while (true)
            {
                string msg;

                msg = Console.ReadLine();
                if (msg == "exit")
                {
                    return;
                }
            }
        }

        private static void OtherMain()
        {
            var connectionBuilder =
                new ConnectionBuilder<TestContractImplementation>()
                    .UseDeserializer<string>(new UnicodeDeserializer());


            var server = new TcpChannelServer<TestContractImplementation>(connectionBuilder, new IPEndPoint(IPAddress.Any, 1111));
            var server2 = new ChannelServer<TestContractImplementation, TcpChannel>(connectionBuilder, new TcpChanelListener(new IPEndPoint(IPAddress.Any, 1111)));

            var server3 =
                new ConnectionBuilder<TestContractImplementation>()
                    .UseDeserializer<string>(new UnicodeDeserializer())
                    .UseReceiveDispatcher<NotThreadDispatcher>()
                    .CreateTcpServer(IPAddress.Any, 1111);

            server.IsListening = true;

            //...

            server.Close();
        }



        private static void ChannelOnOnDisconnect(LightChannel lightChannel)
        {
            Console.WriteLine("Disconnected!!");
        }

    }
}
