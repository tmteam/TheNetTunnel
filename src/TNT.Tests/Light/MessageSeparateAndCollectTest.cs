using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TNT.Light;
using TNT.Light.Receiving;
using TNT.Light.Sending;

namespace TNT.Tests.Light
{
    [TestFixture()]
    public class MessageSeparateAndCollectTest
    {
        [Test]
        public void BigArraySeparateAndCollect_originAndCollectedAreEqual()
        {
            byte[] originArray = Enumerable.Range(1, 10000).Select(s => (byte)(s % 255)).ToArray();

            var collectedArray = SeparateAndCollect(originArray);
            Assert.IsNotNull(collectedArray, "Light message was not collected");
            CollectionAssert.AreEqual(originArray, collectedArray);
        }
        [Test]
        public void EmptyArraySeparateAndCollect_originAndCollectedAreEqual()
        {
            byte[] originArray = new byte[0];
            var collectedArray = SeparateAndCollect(originArray);
            Assert.IsNotNull(collectedArray, "Light message was not collected");
            CollectionAssert.AreEqual(originArray, collectedArray);
        }
        [Test]
        public void SingleElementArraySeparateAndCollect_originAndCollectedAreEqual()
        {
            byte[] originArray = new byte[] {1};
            var collectedArray = SeparateAndCollect(originArray);
            Assert.IsNotNull(collectedArray, "Light message was not collected");
            CollectionAssert.AreEqual(originArray, collectedArray);
        }
        [Test]
        public void SmallArraySeparateAndCollect_originAndCollectedAreEqual()
        {
            byte[] originArray = new byte[] { 1,2,3,4,5,6,7 };
            var collectedArray = SeparateAndCollect(originArray);
            Assert.IsNotNull(collectedArray, "Light message was not collected");
            CollectionAssert.AreEqual(originArray, collectedArray);
        }

        private static byte[] SeparateAndCollect(byte[] originArray)
        {
            List<byte[]> quants = new List<byte[]>();

            using (var stream = new MemoryStream(originArray))
            {
                byte[] quant = null;
                var separator = new MessageSeparator(stream, 42, 1024);
                while (separator.TryNext(out quant))
                {
                    quants.Add(quant);
                }
            }
            var collector = new MessageCollector();

            foreach (var quant in quants)
            {
                if (collector.Collect(quant, 0))
                {
                    using (var collected = collector.GetLightMessageStream())
                    {
                        return collected.ToArray();
                    }

                }
            }
            return null;
        }
    }
}
