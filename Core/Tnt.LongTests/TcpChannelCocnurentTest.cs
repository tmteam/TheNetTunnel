using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TNT.IntegrationTests;
using TNT.Tcp;

namespace Tnt.LongTests
{
     [TestFixture]
    public class TcpChannelCocnurentTest
    {
        [TestCase(100000, 100, 13343)]
        [TestCase(120000, 100, 13344)]
        [TestCase(150000, 100, 13345)]
        [TestCase(170000, 100, 13346)]
        [TestCase(200000, 100, 13347)]
       // [TestCase(400000, 100, 13348)]
        public void SendAndReceiveViaTcpTest(int length, int concurentLevel, int port)
        {
            TcpClient client;
            TcpClient serverClient;
            IntegrationTestsHelper.CreateTwoConnectedTcpClients(port, out client, out serverClient);

            var channelA = new TcpChannel(client);
            var channelB = new TcpChannel(serverClient);
            List<byte> recievedList = new List<byte>(length);
            int doneThreads = 0;

            Exception innerException = null;
            channelB.OnReceive+=(_, msg)=>{
                recievedList.AddRange(msg);
                if (recievedList.Count >= length)
                {
                    byte lastValue = recievedList[0];
                    for (int i = 0; i < length; i++)
                    {
                        var val = recievedList[0];
                        recievedList.RemoveAt(0);
                            try { Assert.AreEqual(lastValue, val,"Value is not as expected sience index " + i);}
                            catch(Exception e) {innerException = e;}
                    }
                    doneThreads++;
                }
            };
            channelB.AllowReceive = true;
            
            IntegrationTestsHelper.RunInParrallel<byte[]>(concurentLevel,
                  (i) =>
                  {
                     return IntegrationTestsHelper.CreateArray(length, (byte)i);
                  },
                  (i, msg) =>
                  {
                      channelA.Write(msg,0,msg.Length);
                  });
            IntegrationTestsHelper.WaitOrThrow(() => (doneThreads >= concurentLevel), () => innerException);
            channelA.Disconnect();
            channelB.Disconnect();
        }
    }
}
