using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using TNT.Tcp;
using TNT.Transport;

namespace Tnt.LongTests;

[TestFixture]
public class TransporterConcurrentTest
{

    // [TestCase(1000000, 100)]
    public void SendsAndReceivesViaMockConnection(int length, int ConcurrentLevel)
    {
        var pair = TNT.Testing.TntTestHelper.CreateChannelPair();

        pair.ConnectAndStartReceiving();

        var sendtransporter = new Transporter(pair.ChannelA);
        var receiveTransporwe = new Transporter(pair.ChannelB);
        int doneThreads = 0;
        Exception inThreadsException = null;

        receiveTransporwe.OnReceive += (_, arg) =>
        {
            var buffer = new byte[length];
            arg.Position = 0;
            arg.Read(buffer, 0, length);
            byte lastValue = buffer.Last();
            for (int i = 0; i < length; i++)
            {
                try
                {
                    Assert.AreEqual(lastValue, buffer[i], "Value is not as expected sience index " + i);
                }
                catch (Exception e)
                {
                    inThreadsException = e;
                }
            }
            doneThreads++;
        };

        IntegrationTestsHelper.RunInParrallel(ConcurrentLevel,
            initializeAction: i =>
            {
                byte[] array = IntegrationTestsHelper.CreateArray(length, (byte)i);
                var stream = sendtransporter.CreateStreamForSend();
                stream.Write(array, 0, array.Length);
                return stream;
            },
            action: (i, stream) =>
            {
                sendtransporter.Write(stream);
            });

        IntegrationTestsHelper.WaitOrThrow(() => doneThreads != ConcurrentLevel - 1, () => inThreadsException);
        pair.Disconnect();    
    }

    [TestCase(100000, 100, 12343)]
    [TestCase(120000, 100, 12344)]
    [TestCase(150000, 100, 12345)]
    [TestCase(170000, 100, 12346)]
    [TestCase(200000, 100, 12347)]
    [TestCase(400000, 100, 12348)]
    // [TestCase(1000000, 100, 12349)]
    public void SendsAndReceivesViaTcpConnection(int length, int ConcurrentLevel, int port)
    {
        IntegrationTestsHelper.CreateTwoConnectedTcpClients(port, out var client, out var serverClient);

        var sendChannel = new TcpChannel(client);
        var receiveChannel = new TcpChannel(serverClient);
        sendChannel.AllowReceive = true;
        receiveChannel.AllowReceive = true;

        var sendtransporter = new Transporter(sendChannel);
        var receiveTransporwe = new Transporter(receiveChannel);

        int doneThreads = 0;
        Exception inThreadsException = null;

        receiveTransporwe.OnReceive += (_, arg) =>
        {
            var buffer = new byte[length];
            arg.Position = 0;
            arg.Read(buffer, 0, length);
            byte lastValue = buffer.Last();
            for (int i = 0; i < length; i++)
            {
                try
                {
                    Assert.AreEqual(lastValue, buffer[i], "Value is not as expected sience index "+ i);
                }
                catch (Exception e)
                {
                    inThreadsException = e;
                }
            }
            doneThreads++;
        };

        IntegrationTestsHelper.RunInParrallel(ConcurrentLevel,
            i =>
            {
                byte[] array = IntegrationTestsHelper.CreateArray(length, (byte)i);
                var stream = sendtransporter.CreateStreamForSend();
                stream.Write(array, 0, array.Length);
                return stream;
            },
            (i, stream) =>
            {
                sendtransporter.Write(stream);
            });

        IntegrationTestsHelper.WaitOrThrow(() => doneThreads != ConcurrentLevel - 1, () => inThreadsException);
        client.Dispose();
        serverClient.Dispose();
    }

       
}