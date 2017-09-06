using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TNT.IntegrationTests;
using TNT.IntegrationTests.ContractMocks;
using TNT.Presentation;
using TNT.Presentation.Serializers;
using TNT.Transport;

namespace Tnt.LongTests
{
    [TestFixture]
    public class ConcurentSenderTest
    {
        [TestCase(1000000, 100)]
      //  [MaxTime(1000)]
        public void Sends(int length, int concurentLevel)
        {
            var channel = new TNT.Testing.TestChannel(false);
            channel.ImmitateConnect();

            var transporter = new Transporter(channel);

            int id = 1;
            var serializer = new ByteArraySerializer();
            var sender = new Sender(transporter, new Dictionary<int, ISerializer> { { id, serializer } });

            int expectedHeadLength = 6;
            var start = new ManualResetEvent(false);
            int doneThreads = 0;
            
            for (int i = 0; i < concurentLevel; i++)
            {
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    byte[] array = CreateArray(length, (byte)i);
                    start.WaitOne();
                    sender.Say(id, new object[] { array });
                });
            }
            channel.OnWrited += (_, arg) =>
            {
                Assert.AreEqual(expectedHeadLength + length, arg.Length);
                byte lastValue = arg.Last();
                for (int i = expectedHeadLength; i < expectedHeadLength + length; i++)
                {
                    Assert.AreEqual(lastValue, arg[i]);
                }
                doneThreads++;
            };

            start.Set();

            while (doneThreads != concurentLevel - 1)
            {
                Thread.Sleep(1);
            }
        }

        private static byte[] CreateArray(int length, byte value)
        {
            var array = new byte[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = value;
            }

            return array;
        }
    }
}
