using System;
using System.Linq;
using NUnit.Framework;
using TNT.Transport;

namespace TNT.Core.Tests
{
    [TestFixture]
    public class ReceivePduQueueTest
    {
        [Test]
        public void EmptyMessage_DissasemblesAndCollects_byOnePacket()
        {
            var message = CreateMessageForSend(new byte[0]);
            
            var queue = new ReceivePduQueue();
            queue.Enqueue(message);

            var result = queue.DequeueOrNull();
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.ToArray());
        }
        [Test]
        public void NonEmptyMessage_DissasemblesAndCollects_byOnePacket()
        {
            byte[] array = new byte[] {1,2,3,4,5,6,7};
            var message = CreateMessageForSend(array);

            var queue = new ReceivePduQueue();
            queue.Enqueue(message);

            var result = queue.DequeueOrNull();
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(array, result.ToArray());
        }
        [Test]
        public void TwoNonEmptyMessages_DissasembledAndCollectedbyOnePacket_SecondCollectedWell()
        {
            byte[] array1 = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            byte[] array2 = new byte[] { 5,3,0, 1, 2, 3, 4, 5, 6, 7 };

            var message1 = CreateMessageForSend(array1);
            var message2 = CreateMessageForSend(array2);

            var queue = new ReceivePduQueue();
            queue.Enqueue(message1.Concat(message2).ToArray());

            queue.DequeueOrNull();
            var result = queue.DequeueOrNull();
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(array2, result.ToArray());
        }
        [Test]
        public void TwoNonEmptyMessages_DissasembledAndCollectedbyOnePacket_FirstCollectedWell()
        {
            byte[] array1 = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            byte[] array2 = new byte[] { 5, 3, 0, 1, 2, 3, 4, 5, 6, 7 };

            var message1 = CreateMessageForSend(array1);
            var message2 = CreateMessageForSend(array2);

            var queue = new ReceivePduQueue();
            queue.Enqueue(message1.Concat(message2).ToArray());

            var result = queue.DequeueOrNull();
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(array1, result.ToArray());
        }
        [Test]
        public void EmptyMessage_DissasemblesAndCollects_ByteByByte()
        {
            var queue = new ReceivePduQueue();

            var message = CreateMessageForSend(new byte[0]);
            foreach (var b in message)
                queue.Enqueue(new byte[] { b });

            var result = queue.DequeueOrNull();
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.ToArray());
        }
        [Test]
        public void NonEmptyMessage_DissasemblesAndCollects_ByteByByte()
        {
            byte[] array = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            var queue = new ReceivePduQueue();

            var message = CreateMessageForSend(array);
            foreach (var b in message)
                queue.Enqueue(new byte[] { b });

            var result = queue.DequeueOrNull();
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(array, result.ToArray());
        }
        [Test]
        public void TwoNonEmptyMessages_DissasemblesAndCollectsByteByByte_FirstCollectedWell()
        {
            byte[] array1 = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            byte[] array2 = new byte[] { 5, 3, 0, 1, 2, 3, 4, 5, 6, 7 };

            var message1 = CreateMessageForSend(array1);
            var message2 = CreateMessageForSend(array2);

            var queue = new ReceivePduQueue();

            foreach (var b in message1.Concat(message2))
                queue.Enqueue(new byte[] { b });
            var result = queue.DequeueOrNull();
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(array1, result.ToArray());
        }
        [Test]
        public void TwoNonEmptyMessages_DissasemblesAndCollectsByteByByte_SecondCollectedWell()
        {
            byte[] array1 = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            byte[] array2 = new byte[] { 5, 3, 0, 1, 2, 3, 4, 5, 6, 7 };

            var message1 = CreateMessageForSend(array1);
            var message2 = CreateMessageForSend(array2);

            var queue = new ReceivePduQueue();

            foreach (var b in message1.Concat(message2))
                queue.Enqueue(new byte[] { b });
            queue.DequeueOrNull();
            var result = queue.DequeueOrNull();
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(array2, result.ToArray());
        }
        [Test]
        public void NoMessageEnqued_DequeueOrNull_returnsNull()
        {
            var queue = new ReceivePduQueue();
            var result = queue.DequeueOrNull();
            Assert.IsNull(result);
        }
        [Test]
        public void UndoneHeadEnqued_DequeueOrNull_returnsNull()
        {
            var queue = new ReceivePduQueue();
            queue.Enqueue(new byte[2] {1,2});
            var result = queue.DequeueOrNull();
            Assert.IsNull(result);
        }
        [Test]
        public void UndoneMessageEnqued_DequeueOrNull_returnsNull()
        {

            byte[] array = new byte[] { 1, 2, 3, 4, 5, 6, 7 };

            var message = CreateMessageForSend(array);
            var queue = new ReceivePduQueue();
            Array.Resize(ref message, 5);
            queue.Enqueue(message);
            var result = queue.DequeueOrNull();
            Assert.IsNull(result);
        }


        private static byte[] CreateMessageForSend(byte[] array)
        {

            var sendStreamManager = new SendStreamManager();
            var stream = sendStreamManager.CreateStreamForSend();

            stream.Write(array, 0, array.Length);
            sendStreamManager.PrepareForSending(stream);
            stream.Position = 0;
            return stream.ToArray();
        }

      
    }
}
