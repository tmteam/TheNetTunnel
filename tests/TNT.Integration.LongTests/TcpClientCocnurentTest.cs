using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Tnt.LongTests
{
    [TestFixture]
    public class TcpClientCocnurentTest
    {
        [TestCase(100000, 100, 13343)]
        [TestCase(120000, 100, 13344)]
        [TestCase(150000, 100, 13345)]
        [TestCase(170000, 100, 13346)]
        [TestCase(200000, 100, 13347)]
       // [TestCase(400000, 100, 13348)]
        public void SendAndReceiveViaTcpTest(int length, int ConcurrentLevel, int port)
        {
            TcpClient client;
            TcpClient serverClient;
            IntegrationTestsHelper.CreateTwoConnectedTcpClients(port, out client, out serverClient);

            var receiveTask = ReciveAndCheck(serverClient, length, ConcurrentLevel);

            
            IntegrationTestsHelper.RunInParrallel<byte[]>(ConcurrentLevel, i => {
                     return IntegrationTestsHelper.CreateArray(length, (byte)i);
                  },
                  (i, msg) =>
                  {
                      lock (client)
                      {
                          //var stream = client.GetStream();
                          //var size = client.SendBufferSize;
                          //for (int m = 0; m < msg.Length; i += size)
                          //{
                          //    int len = Math.Min(size, msg.Length-m);
                          //    client.GetStream().Write(msg, m, msg.Length);
                          //}
                          client.GetStream().Write(msg, 0, msg.Length);
                      }
                  });
            try
            {
                receiveTask.Wait();
            }
            catch(AggregateException e)
            {
                throw e.InnerException;
            }
        }
        async Task ReciveAndCheck(TcpClient client, int length, int expectedReceivings)
        {
            byte[] receiveBuffer = new byte[length];
            List<byte> recievedList = new List<byte>(length);
            int doneThreads = 0;
            while (doneThreads<expectedReceivings)
            {
              
                var receivedLength = await client.GetStream().ReadAsync(receiveBuffer, 0, length);
                recievedList.AddRange(receiveBuffer.Take(receivedLength));
                if(recievedList.Count>=length)
                {
                    byte lastValue = recievedList[0];
                    for (int i = 0; i < length; i++)
                    {
                        var val = recievedList[0];
                        recievedList.RemoveAt(0);
                        if (lastValue!= val)
                            Assert.Fail("Value is not as expected sience index " + i);
                    }
                    doneThreads++;
                }
            }
        }
    }
}
