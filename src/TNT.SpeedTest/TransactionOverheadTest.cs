using System.Diagnostics;
using System.Threading;
using TNT.SpeedTest.Contracts;
using TNT.Transport;

namespace TNT.SpeedTest
{
    public class TransactionOverheadTest
    {
        private readonly ISpeedTestContract _proxy;
        private readonly IChannel _channel;
        private readonly Output _output;

        public TransactionOverheadTest(ISpeedTestContract proxy, IChannel channel, Output output)
        {
            _proxy = proxy;
            _channel = channel;
            _output = output;
        }
        public void MeasureOutputOverhead()
        {
            _output.WriteLine("Output overhead measuring");

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

            _output.WriteLine(
                $"    Delay: {(sw.ElapsedMilliseconds * 1000) / (double)iterationsCount} microseconds");
            _output.WriteLine(
                $"    Ticks: {sw.ElapsedTicks / iterationsCount}");
            _output.WriteLine(
                $"    Sent by client per transaction: {(_channel.BytesSent - sentCounter) / iterationsCount} b");
            _output.WriteLine(
                $"    Received by client per transaction: {(_channel.BytesReceived - receivedCounter) / iterationsCount} b");
        }
        public void MeasureTransactionOverhead()
        {
            _output.WriteLine("Transaction overhead measuring");

            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            int iterationsCount = 100000;
            for (int i = 0; i < iterationsCount; i++)
                _proxy.AskForTrue();

            sw.Stop();
            _output.WriteLine($"    Delay: {(sw.ElapsedMilliseconds * 1000) / (double)iterationsCount} microseconds");
            _output.WriteLine($"    Ticks: { sw.ElapsedTicks / iterationsCount}");
            _output.WriteLine($"    Sent by client per transaction: {(_channel.BytesSent - sentCounter) / (double)iterationsCount} b");
            _output.WriteLine($"    Received by client per transaction: {(_channel.BytesReceived - receivedCounter) / (double)iterationsCount} b");
        }
    }
}
