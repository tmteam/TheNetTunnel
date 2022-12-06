using System;
using System.Diagnostics;
using System.Threading;
using TNT.SpeedTest.Contracts;
using TNT.Transport;

namespace TNT.SpeedTest.OutputBandwidth;

public class OutputBandwithTest<T>
{
    private readonly IChannel _channel;
    private readonly ISpeedTestContract _contract;
    private readonly Func<int, T> _dataGenerator;
    private readonly Action<int, T> _sendProcedure;

    public OutputBandwithTest(IChannel channel, ISpeedTestContract contract, Func<int, T> dataGenerator, Action<int, T> sendProcedure)
    {
        _channel = channel;
        _contract = contract;
        _dataGenerator = dataGenerator;
        _sendProcedure = sendProcedure;
    }
    public OutputBandwithTestResults Test(int size, int iterationsCount)
    {
        GC.Collect(1);
        int sentCounter = _channel.BytesSent;
        int receivedCounter = _channel.BytesReceived;

        _contract.SubscribeForSayCalled(iterationsCount);
        var allDataReceivedByServer = new ManualResetEvent(false);
        _contract.SaysCallsCountReceived += () => allDataReceivedByServer.Set();

        var packet = _dataGenerator(size);

        var send = new Stopwatch();
        var sendandReceive = new Stopwatch();

        send.Start();
        sendandReceive.Start();

        _sendProcedure(iterationsCount, packet);

        send.Stop();
        allDataReceivedByServer.WaitOne();
        sendandReceive.Stop();

        _contract.SaysCallsCountReceived = null;

        return new OutputBandwithTestResults
        {
            ElaspedMilisecondsForSendOnly = send.ElapsedMilliseconds,
            ElaspedMilisecondsForSendAndReceive = sendandReceive.ElapsedMilliseconds,
            Iterations = iterationsCount,
            Size = size,
            TotalSent     = _channel.BytesSent -sentCounter,
            TotalReceived = _channel.BytesReceived -receivedCounter,
        };

    }

      
}