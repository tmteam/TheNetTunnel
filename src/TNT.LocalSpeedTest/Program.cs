using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TNT.Api;
using TNT.LocalSpeedTest.Contracts;
using TNT.Tcp;
using TNT.Testing;
using TNT.Transport;

namespace TNT.LocalSpeedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test started");

            TestDirectTestConnection();
            Console.WriteLine();
            Console.WriteLine();

            TestLocalhost();
            Console.WriteLine();
            Console.WriteLine("MeasurementDone");
            Console.ReadLine();
        }

        private static void TestDirectTestConnection()
        {
            Console.WriteLine("-------------Direct test mock test--------------");

            var pair = TntTestHelper.CreateThreadlessChannelPair();
            var proxy = TntBuilder
                .UseContract<ISpeedTestContract>()
                .UseChannel(pair.CahnnelA)
                .Build();

            var origin = TntBuilder
                 .UseContract<ISpeedTestContract, SpeedTestContract>()
                .UseChannel(pair.ChannelB)
                .Build();
            pair.ConnectAndStartReceiving();

            Test(proxy);

            pair.Disconnect();
        }

        private static void TestLocalhost()
        {
            Console.WriteLine("-------------Localhost test--------------");
            using (var server = TntBuilder
                .UseContract<ISpeedTestContract, SpeedTestContract>()
                .CreateTcpServer(IPAddress.Loopback, 12345))
            {
                server.IsListening = true;

                using (var client = TntBuilder
                    .UseContract<ISpeedTestContract>()
                    .CreateTcpClientConnection(IPAddress.Loopback, 12345))
                {
                    Test(client);
                }

            }
        }

        private static void Test(IConnection<ISpeedTestContract, IChannel> client)
        {
            client.Contract.AskForTrue();
            client.Contract.AskBytesEcho(new byte[] { 1, 2, 3, });
            client.Contract.AskIntegersEcho(new int[] { 1, 2, 3, });
            client.Contract.AskTextEcho("taram pam pam");
            client.Contract.SayBytes(new byte[] { 1, 2, 3, });
            client.Contract.SayProtoStructEcho(new ProtoStruct());
            client.Contract.AskProtoStructEcho(new ProtoStruct());

            var measurement = new SpeedTestMeasurement(client.Contract, client.Channel);
            measurement.Measure();
        }
    }
}
