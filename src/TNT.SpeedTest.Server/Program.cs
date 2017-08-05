using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TNT.Api;
using TNT.Presentation.ReceiveDispatching;
using TNT.SpeedTest.Contracts;
using TNT.Tcp;

namespace TNT.SpeedTest.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 24731;
            var server = TntBuilder
                .UseContract<ISpeedTestContract, SpeedTestContract>()
                .UseReceiveDispatcher<ConveyorDispatcher>()
                .CreateTcpServer(IPAddress.Any, port);

            server.IsListening = true;
            Console.WriteLine($"Speed test server opened at port {port}");

            while (true)
            {
                Console.WriteLine("Write \"stop\" to exit");
                if (Console.ReadLine().ToLower() == "stop")
                {
                    server.Close();
                    Console.WriteLine("Server stopped");
                    return;
                }
            }

        }
    }
}
