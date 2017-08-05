using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ProtoBuf;
using TNT.LocalSpeedTest.Contracts;
using TNT.Transport;

namespace TNT.LocalSpeedTest
{
   /* public class SpeedTestMeasurement
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
            OneSideBandwidthTest(packetSize: 50000, iterationsCount: 1000);
            GC.Collect(1);
            OneSideBandwidthTest(packetSize: 60000, iterationsCount: 1000);
            GC.Collect(1);
            OneSideBandwidthTest(packetSize: 70000, iterationsCount: 1000);
            GC.Collect(1);
            OneSideBandwidthTest(packetSize: 100000, iterationsCount: 1000);
            GC.Collect(1);
            OneSideBandwidthTest(packetSize: 500000, iterationsCount: 500);
            GC.Collect(1);
            OneSideBandwidthTest(packetSize: 1000000, iterationsCount: 50);
            GC.Collect(1);
            Console.WriteLine();
            TwoSideBandwidthTest(packetSize: 2, iterationsCount: 100000);
            GC.Collect(1);
            TwoSideBandwidthTest(packetSize: 1000, iterationsCount: 10000);
            GC.Collect(1);
            TwoSideBandwidthTest(packetSize: 10000, iterationsCount: 1000);
            GC.Collect(1);
            TwoSideBandwidthTest(packetSize: 50000, iterationsCount: 1000);
            GC.Collect(1);
            TwoSideBandwidthTest(packetSize: 100000, iterationsCount: 1000);
            GC.Collect(1);
            TwoSideBandwidthTest(packetSize: 500000, iterationsCount: 500);

            GC.Collect(1);
            TwoSideBandwidthTest(packetSize: 1000000, iterationsCount: 20);


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
            ProtobuffNetSerializeTest(1, 10000);
            GC.Collect(1);
            ProtobuffNetSerializeTest(100, 1000);
            GC.Collect(1);
            ProtobuffNetSerializeTest(1000, 1000);
            GC.Collect(1);
            ProtobuffNetSerializeTest(10000, 100);
            GC.Collect(1);
            ProtobuffNetSerializeTest(100000, 10);
            GC.Collect(1);
            ProtobuffNetSerializeTest(1000000, 1);

            Console.WriteLine();
            GC.Collect(1);
            OneSideProtobuffTest(1, 10000);
            GC.Collect(1);
            OneSideProtobuffTest(100, 1000);
            GC.Collect(1);
            OneSideProtobuffTest(1000, 1000);
            GC.Collect(1);
            OneSideProtobuffTest(10000, 100);
            GC.Collect(1);
            OneSideProtobuffTest(100000, 10);
            GC.Collect(1);
            OneSideProtobuffTest(1000000, 1);
            Console.WriteLine();
            GC.Collect(1);
            TwoSideProtobuffTest(1, 10000);
            GC.Collect(1);
            TwoSideProtobuffTest(100, 1000);
            GC.Collect(1);
            TwoSideProtobuffTest(1000, 1000);
            GC.Collect(1);
            TwoSideProtobuffTest(10000, 10);
            GC.Collect(1);
            TwoSideProtobuffTest(100000, 10);
            GC.Collect(1);
            TwoSideProtobuffTest(1000000, 1);
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

        public void ProtobuffNetSerializeTest(int memeberSize, int iterationsCount)
        {
            Console.Write($"protobuff-net test {memeberSize:   0} items... [Calculating]");

            var packet = GenerateProtoStruct(memeberSize);
            var serializedLength = 0;
            ProtoBuf.Serializer.PrepareSerializer<ProtoStruct>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Act:
            for (int i = 0; i < iterationsCount; i++)
            {
                using (var stream = new MemoryStream())
                {
                    stream.Position = 0;
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<ProtoStruct>(stream, packet, PrefixStyle.None);
                    stream.Position = 0;
                    serializedLength += (int) stream.Length;
                    var result = ProtoBuf.Serializer.DeserializeWithLengthPrefix<ProtoStruct>(stream, PrefixStyle.None);
                }
            }
            sw.Stop();

            Console.CursorLeft = 0;
            var megabytesSerialized = serializedLength / (1024d * 1024d);

            Console.WriteLine(
                $"protobuff-net test {memeberSize:   0} items: { (megabytesSerialized / sw.Elapsed.TotalSeconds):0.00} megabytes per second");
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
            Stopwatch send = new Stopwatch();
            Stopwatch sendandReceive = new Stopwatch();
            send.Start();

            sendandReceive.Start();
            //Act:
            for (int i = 0; i < iterationsCount; i++)
                _proxy.SayBytes(packet);
            send.Stop();
            mre.WaitOne();
            sendandReceive.Stop();
            _proxy.SaysCallsCountReceived = null;

            Console.CursorLeft = 0;
            var megabytesSend = (_channel.BytesSent - sentCounter) / (1024d * 1024d);

            Console.WriteLine(
                $"One-side Send Bandwidth for {packetSize:0000000} bytes: { (megabytesSend / send.Elapsed.TotalSeconds):0.00} megabytes per second");

            Console.WriteLine(
                $"One-side IO Bandwidth for {packetSize:0000000} bytes: { (megabytesSend / sendandReceive.Elapsed.TotalSeconds):0.00} megabytes per second");
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

    }*/
}
