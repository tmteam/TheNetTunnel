using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TNT.Presentation;
using TNT.Transport;

namespace Tnt.LongTests
{
    [TestFixture]
    public class TransporterConcurentTest
    {

        [TestCase(1000000, 100)]
        //  [MaxTime(1000)]
        public void SendsAndReceivesViaMockConnection(int length, int concurentLevel)
        {
            var pair = TNT.Testing.TntTestHelper.CreateChannelPair();

            pair.ConnectAndStartReceiving();

            var sendtransporter = new Transporter(pair.CahnnelA);
            var receiveTransporwe = new Transporter(pair.ChannelB);
            var start = new ManualResetEvent(false);
            int doneThreads = 0;

            for (int i = 0; i < concurentLevel; i++)
            {
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    byte[] array = CreateArray(length, (byte)i);
                    var stream = sendtransporter.CreateStreamForSend();
                    stream.Write(array, 0, array.Length);
                    start.WaitOne();
                    sendtransporter.Write(stream);
                });
            }
            receiveTransporwe.OnReceive+=(_,arg)=>
            {
                var buffer = new byte[length];
                arg.Position = 0;
                arg.Read(buffer, 0, length);
                byte lastValue = buffer.Last();
                for (int i = 0; i < length; i++)
                {
                    Assert.AreEqual(lastValue, buffer[i]);
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
