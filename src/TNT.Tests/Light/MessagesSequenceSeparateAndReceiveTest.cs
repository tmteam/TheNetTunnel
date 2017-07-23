using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TNT.Transport.Receiving;
using TNT.Transport.Sending;

namespace TNT.Tests.Light
{

    [TestFixture()]
    public class MessagesSequenceSeparateAndReceiveTest
    {
        [Test]
        public void FIFO_SeparateAndCollectEmptyArray_CollectedEqualToOrigin()
        {
            byte[] originArray = new byte[0];
            var collectedArray = SeparateAndCollect(new FIFOSendPduBehaviour(),  originArray);
            Assert.IsNotNull(collectedArray);
            CollectionAssert.AreEqual(originArray,collectedArray);
        }
        [Test]
        public void FIFO_SeparateAndCollectSmallArray_CollectedEqualToOrigin()
        {
            byte[] originArray = new byte[] {1,2,3,4};
            var collectedArray = SeparateAndCollect(new FIFOSendPduBehaviour(), originArray);
            Assert.IsNotNull(collectedArray);
            CollectionAssert.AreEqual(originArray, collectedArray);
        }
        [Test]
        public void FIFO_SeparateAndCollectBigArray_CollectedEqualToOrigin()
        {
            byte[] originArray = Enumerable.Range(1, 10000).Select(s => (byte)(s % 255)).ToArray();
            var collectedArray = SeparateAndCollect(new FIFOSendPduBehaviour(), originArray);
            Assert.IsNotNull(collectedArray);
            CollectionAssert.AreEqual(originArray, collectedArray);
        }

        [Test]
        public void Mix_SeparateAndCollectEmptyArray_CollectedEqualToOrigin()
        {
            byte[] originArray = new byte[0];
            var collectedArray = SeparateAndCollect(new MixedSendPduBehaviour(), originArray);
            Assert.IsNotNull(collectedArray);
            CollectionAssert.AreEqual(originArray, collectedArray);
        }
        [Test]
        public void Mix_SeparateAndCollectSmallArray_CollectedEqualToOrigin()
        {
            byte[] originArray = new byte[] { 1, 2, 3, 4 };
            var collectedArray = SeparateAndCollect(new MixedSendPduBehaviour(), originArray);
            Assert.IsNotNull(collectedArray);
            CollectionAssert.AreEqual(originArray, collectedArray);
        }
        [Test]
        public void Mix_SeparateAndCollectBigArray_CollectedEqualToOrigin()
        {
            byte[] originArray = Enumerable.Range(1, 10000).Select(s => (byte)(s % 255)).ToArray();
            var collectedArray = SeparateAndCollect(new MixedSendPduBehaviour(), originArray);
            Assert.IsNotNull(collectedArray);
            CollectionAssert.AreEqual(originArray, collectedArray);
        }
        [Test]
        public void FIFO_SeparateAndCollectManyMessages_CollectedEqualToOrigins()
        {
            List<byte[]> originMessages = new List<byte[]>
            {
                Enumerable.Range(1, 10000).Select(s => (byte) (s % 255)).ToArray(),
                new byte[] {1, 2, 3, 4},
                new byte[0],
                new byte[] {1, 2, 3, 4},
            };

            var separator = new FIFOSendPduBehaviour();
            var collector = new ReceivePduQueue();

            foreach (var origin in originMessages.Select(o => new MemoryStream(o)))
            {
                separator.Enqueue(origin);
            }
            
            byte[] quantum = null;
            int msgId = 0;
            while (separator.TryDequeue(out quantum, out msgId))
            {
                collector.Enqueue(quantum);
            }
            for (int i = 0; i < originMessages.Count; i++)
            {
                var collected = collector.DequeueOrNull();
                Assert.IsNotNull(collected);
                CollectionAssert.AreEqual(originMessages[i], collected.ToArray());
            }
        }
        [Test]
        public void Mix_SeparateAndCollectManyMessages_CollectedSameToOrigins()
        {
            List<byte[]> originMessages = new List<byte[]>
            {
                new byte[] {1, 2},
                Enumerable.Range(1, 10000).Select(s => (byte) (s % 255)).ToArray(),
                new byte[] {1, 2, 3, 4},
                new byte[0],
                Enumerable.Range(1, 5000).Select(s => (byte) (s % 255)).ToArray(),
            };

            var separator = new MixedSendPduBehaviour();
            var collector = new ReceivePduQueue();

            foreach (var origin in originMessages.Select(o => new MemoryStream(o)))
            {
                separator.Enqueue(origin);
            }

            byte[] quantum = null;
            int msgId = 0;
            while (separator.TryDequeue(out quantum, out msgId))
            {
                collector.Enqueue(quantum);
            }


            while (!collector.IsEmpty)
            {
                var collected = collector.DequeueOrNull();
                Assert.IsNotNull(collected);
                var collectedArray = collected.ToArray();
                var origin = originMessages.FirstOrDefault(o => ArraysAreEqual(o, collectedArray));

                Assert.IsNotNull(origin);
                originMessages.Remove(origin);
            }
            if(originMessages.Any())
                Assert.Fail("Не все сообщения доставленны");
        }

        private bool ArraysAreEqual(byte[] origin, byte[] other)
        {
            if (origin.Length != other.Length)
                return false;
            for (int i = 0; i < origin.Length; i++)
            {
                if (origin[i] != other[i])
                    return false;
            }
            return true;
        }

        private static byte[] SeparateAndCollect(ISendPduBehaviour separator, byte[] originArray)
        {
            var stream = new MemoryStream(originArray);

            var collector = new ReceivePduQueue();

            separator.Enqueue(stream);

            byte[] quantum = null;
            int msgId = 0;
            while (separator.TryDequeue(out quantum, out msgId))
            {
                collector.Enqueue(quantum);
            }
            var collected = collector.DequeueOrNull();

            if (collected == null)
                return null;


            return collected.ToArray();

        }
    }
}
