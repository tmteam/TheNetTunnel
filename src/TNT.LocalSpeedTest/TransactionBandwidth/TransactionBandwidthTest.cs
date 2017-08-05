using System;
using System.Diagnostics;
using System.Threading;
using TNT.LocalSpeedTest.Contracts;
using TNT.Transport;

namespace TNT.LocalSpeedTest.TransactionBandwidth
{
    public class TransactionBandwithTest<T>
    {
        private readonly IChannel _channel;
        private readonly ISpeedTestContract _contract;
        private readonly Func<int, T> _dataGenerator;
        private readonly Action<int, T> _sendProcedure;

        public TransactionBandwithTest(IChannel channel, ISpeedTestContract contract, Func<int, T> dataGenerator, Action<int, T> sendProcedure)
        {
            _channel = channel;
            _contract = contract;
            _dataGenerator = dataGenerator;
            _sendProcedure = sendProcedure;
        }
        public TransactionBandwidthTestResults Test(int size, int iterationsCount)
        {
            GC.Collect(1);
            int sentCounter = _channel.BytesSent;
            int receivedCounter = _channel.BytesReceived;

            var packet = _dataGenerator(size);

            var sw = new Stopwatch();

            sw.Start();

            _sendProcedure(iterationsCount, packet);

            sw.Stop();

            return new TransactionBandwidthTestResults
            {
                ElaspedMiliseconds = sw.ElapsedMilliseconds,
                Iterations = iterationsCount,
                Size = size,
                TotalSent     = _channel.BytesSent -sentCounter,
                TotalReceived = _channel.BytesReceived -receivedCounter,
            };

        }

      
    }
    
}