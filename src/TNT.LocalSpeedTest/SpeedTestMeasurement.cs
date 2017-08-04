using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TNT.LocalSpeedTest.Contracts;
using TNT.Transport;

namespace TNT.LocalSpeedTest
{
    public class SpeedTestMeasurement
    {
        private readonly ISpeedTestContract _proxy;
        private readonly IChannel _channel;
        private readonly SpeedTestContract _origin;

        public SpeedTestMeasurement(ISpeedTestContract proxy, IChannel channel)
        {
            _proxy = proxy;
            _channel = channel;
        }

        public void Measure()
        {
            Console.WriteLine("Speed test started. ");
            GC.Collect(1);

            Console.WriteLine();
            EmptyOneSideTransactionOverheadTest();
            GC.Collect(1);

            Console.WriteLine();
            EmptyTwoSideTransactionOverheadTest();
            GC.Collect(1);

            Console.WriteLine();
            OneSideBandwidthTest(packetSize: 1, iterationsCount: 100000);
            GC.Collect(1);
            OneSideBandwidthTest(packetSize: 1000, iterationsCount: 10000);
            GC.Collect(1);
            OneSideBandwidthTest(packetSize: 10000, iterationsCount: 1000);
            GC.Collect(1);
            OneSideBandwidthTest(packetSize: 1 * 1024 * 1024, iterationsCount: 50);
            GC.Collect(1);
            Console.WriteLine();
            TwoSideBandwidthTest(packetSize: 2, iterationsCount: 100000);
            GC.Collect(1);
            TwoSideBandwidthTest(packetSize: 1000, iterationsCount: 10000);
            GC.Collect(1);
            TwoSideBandwidthTest(packetSize: 10000, iterationsCount: 1000);
            GC.Collect(1);
            TwoSideBandwidthTest(packetSize: 1 * 1024 * 1024, iterationsCount: 20);


            Console.WriteLine();
            GC.Collect(1);
            OneSideStringTest(1, 10000);
            GC.Collect(1);
            OneSideStringTest(1000, 1000);
            GC.Collect(1);
            OneSideStringTest(10000, 1000);
            GC.Collect(1);
            OneSideStringTest(1000000, 100);
            Console.WriteLine();
            GC.Collect(1);
            TwoSideStringTest(1, 10000);
            GC.Collect(1);
            TwoSideStringTest(1000, 1000);
            GC.Collect(1);
            TwoSideStringTest(10000, 1000);
            GC.Collect(1);
            TwoSideStringTest(1000000, 100);


            Console.WriteLine();
            GC.Collect(1);
            OneSideProtobuffTest(1, 10000);
            GC.Collect(1);
            OneSideProtobuffTest(1000, 1000);
            GC.Collect(1);
            OneSideProtobuffTest(10000, 1000);
            GC.Collect(1);
            OneSideProtobuffTest(1000000, 100);
            Console.WriteLine();
            GC.Collect(1);
            TwoSideProtobuffTest(1, 10000);
            GC.Collect(1);
            TwoSideProtobuffTest(1000, 1000);
            GC.Collect(1);
            TwoSideProtobuffTest(10000, 1000);
            GC.Collect(1);
            TwoSideProtobuffTest(1000000, 100);


        }

        public void OneSideStringTest(int stringLength, int iterationsCount)
        {
            Console.Write($"One-side string of {stringLength:0000000} chars... [Calculating]");

            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

            _proxy.SubscribeForSayCalled(iterationsCount);
            ManualResetEvent mre = new ManualResetEvent(false);
            _proxy.SaysCallsCountReceived += () => mre.Set();

            var packet = GenerateString(stringLength);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Act:
            for (int i = 0; i < iterationsCount; i++)
                _proxy.SayString(packet);
            mre.WaitOne();
            sw.Stop();
            _proxy.SaysCallsCountReceived = null;

            Console.CursorLeft = 0;
            var megabytesSend = (_channel.BytesSent - sentCounter) / (1024d * 1024d);
            Console.WriteLine(
                $"One-side string of {stringLength:0000000} chars: { (megabytesSend / sw.Elapsed.TotalSeconds):0.00} megabytes per second");
        }

        public void TwoSideStringTest(int stringLength, int iterationsCount)
        {
            Console.Write($"Two-side string of {stringLength:000000} chars... [Calculating]");

            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

            _proxy.SubscribeForSayCalled(iterationsCount);

            var packet = GenerateString(stringLength);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Act:
            for (int i = 0; i < iterationsCount; i++)
                _proxy.AskTextEcho(packet);

            sw.Stop();

            Console.CursorLeft = 0;
            var megabytesReceived = (_channel.BytesReceived - receivedCounter) / (1024d * 1024d);
            var megabytesSent = (_channel.BytesSent - sentCounter) / (1024d * 1024d);

            Console.WriteLine(
                $"Two-side string of {stringLength:000000} chars: { ((megabytesReceived + megabytesSent) / sw.Elapsed.TotalSeconds):0.00} megabytes per second");
        }

        public void OneSideProtobuffTest(int memeberSize, int iterationsCount)
        {
            Console.Write($"One-side protobuff test for {memeberSize} relative size... [Calculating]");

            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

            _proxy.SubscribeForSayCalled(iterationsCount);
            ManualResetEvent mre = new ManualResetEvent(false);
            _proxy.SaysCallsCountReceived += () => mre.Set();

            var packet = GenerateProtoStruct(memeberSize);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Act:
            for (int i = 0; i < iterationsCount; i++)
                _proxy.SayProtoStructEcho(packet);
            mre.WaitOne();
            sw.Stop();
            _proxy.SaysCallsCountReceived = null;

            Console.CursorLeft = 0;
            var megabytesSend = (_channel.BytesSent - sentCounter) / (1024d * 1024d);
            Console.WriteLine(
                $"One-side protobuff {memeberSize:0000000} items: { (megabytesSend / sw.Elapsed.TotalSeconds):0.00} megabytes per second");
        }

        public void TwoSideProtobuffTest(int memeberSize, int iterationsCount)
        {
            Console.Write($"Two-side protobuff {memeberSize:   0} items... [Calculating]");

            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

            _proxy.SubscribeForSayCalled(iterationsCount);

            var packet = GenerateProtoStruct(memeberSize);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Act:
            for (int i = 0; i < iterationsCount; i++)
                _proxy.AskProtoStructEcho(packet);

            sw.Stop();

            Console.CursorLeft = 0;
            var megabytesReceived = (_channel.BytesReceived - receivedCounter) / (1024d * 1024d);
            var megabytesSent = (_channel.BytesSent - sentCounter) / (1024d * 1024d);

            Console.WriteLine(
                $"Two-side protobuff {memeberSize:000000} items: { ((megabytesReceived + megabytesSent) / sw.Elapsed.TotalSeconds):0.00} megabytes per second");
        }

        public void OneSideBandwidthTest(int packetSize, int iterationsCount)
        {
            Console.Write($"One-side Bandwidth test for {packetSize} bytes packet... [Calculating]");

            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

            _proxy.SubscribeForSayCalled(iterationsCount);
            ManualResetEvent mre = new ManualResetEvent(false);
            _proxy.SaysCallsCountReceived += () => mre.Set();

            var packet = GenerateArray(packetSize);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Act:
            for (int i = 0; i < iterationsCount; i++)
                _proxy.SayBytes(packet);
            mre.WaitOne();
            sw.Stop();
            _proxy.SaysCallsCountReceived = null;

            Console.CursorLeft = 0;
            var megabytesSend = (_channel.BytesSent - sentCounter) / (1024d * 1024d);
            Console.WriteLine(
                $"One-side Bandwidth for {packetSize:0000000} bytes: { (megabytesSend / sw.Elapsed.TotalSeconds):0.00} megabytes per second");

           
        }

        public void TwoSideBandwidthTest(int packetSize, int iterationsCount)
        {
            Console.Write($"Two-side Bandwidth test for {packetSize} bytes packet... [Calculating]");

            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

            _proxy.SubscribeForSayCalled(iterationsCount);

            var packet = GenerateArray(packetSize);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Act:
            for (int i = 0; i < iterationsCount; i++)
                _proxy.AskBytesEcho(packet);

            sw.Stop();
            _proxy.SaysCallsCountReceived = null;

            Console.CursorLeft = 0;
            var megabytesReceived = (_channel.BytesReceived - receivedCounter) / (1024d * 1024d);
            var megabytesSent = (_channel.BytesSent - sentCounter) / (1024d * 1024d);

            Console.WriteLine(
                $"Two-side Bandwidth for {packetSize:0000000} bytes: { ((megabytesReceived + megabytesSent)/ sw.Elapsed.TotalSeconds):0.00} megabytes per second");
        }

        public void EmptyOneSideTransactionOverheadTest()
        {
            Console.Write("One-side transaction overhead... [Calculating]");

            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

           
            int iterationsCount = 100000;
            _proxy.SubscribeForSayCalled(iterationsCount);
            
            ManualResetEvent mre = new ManualResetEvent(false);
            _proxy.SaysCallsCountReceived += () => mre.Set();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Act:
            for (int i = 0; i < iterationsCount; i++)
                _proxy.SayNothing();

            mre.WaitOne();
            sw.Stop();
            _proxy.SaysCallsCountReceived = null;

            Console.CursorLeft = 0;
            Console.Write($"One-side transaction overhead:                \r\n");
            Console.WriteLine(
                $"    Delay: {(sw.ElapsedMilliseconds * 1000) / (double)iterationsCount} microseconds");
            Console.WriteLine(
                $"    Ticks: {sw.ElapsedTicks / iterationsCount}");
            Console.WriteLine(
                $"    Sent by client per transaction: {(_channel.BytesSent - sentCounter) / iterationsCount} b");
            Console.WriteLine(
                $"    Received by client per transaction: {(_channel.BytesReceived - receivedCounter) / iterationsCount} b");
        }
        private void EmptyTwoSideTransactionOverheadTest()
        {
            Console.Write("Two-side transaction overhead... [Calculating]");

            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            int iterationsCount = 100000;
            for (int i = 0; i < iterationsCount; i++)
                _proxy.AskForTrue();

            sw.Stop();
            Console.CursorLeft = 0;
            Console.Write($"Two-side transaction overhead:                \r\n");
            Console.WriteLine($"    Delay: {(sw.ElapsedMilliseconds * 1000) / (double)iterationsCount} microseconds");
            Console.WriteLine($"    Ticks: { sw.ElapsedTicks / iterationsCount}");
            Console.WriteLine($"    Sent by client per transaction: {(_channel.BytesSent - sentCounter) / (double)iterationsCount} b");
            Console.WriteLine($"    Received by client per transaction: {(_channel.BytesReceived - receivedCounter) / (double)iterationsCount} b");
        }

        private byte[] GenerateArray(int size)
        {
            var rnd = new Random(size);
            var ans = new byte[size];
            rnd.NextBytes(ans);
            return ans;
        }

        private string GenerateString(int size)
        {
            //up to https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c/1344255#1344255
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[size];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
        private ProtoStruct GenerateProtoStruct(int size)
        {
            var rnd = new Random(size);

            List<ProtoStructItem> items = new List<ProtoStructItem>(size);
            for (int i = 0; i < items.Count; i++)
            {
                var str = new ProtoStructItem
                {
                    Byte = (byte) (rnd.Next() % 0xFF),
                    Integer = rnd.Next(),
                    IntegerArray = new int[4] {rnd.Next(), rnd.Next(), rnd.Next(), rnd.Next()},
                    Long = rnd.Next(),
                    Text = "piu piu, superfast",
                    Time = DateTime.Now
                };
                items.Add(str);
            }
            return new ProtoStruct()
            {
                Members = items.ToArray()
            };
        }

    }
}
