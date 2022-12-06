using TNT.SpeedTest.Contracts;
using TNT.Transport;

namespace TNT.SpeedTest.TransactionBandwidth;

public class TransactionMeasurement
{
    private readonly ISpeedTestContract _proxy;
    private readonly IChannel _channel;
    private readonly Output _output;

    public TransactionMeasurement(ISpeedTestContract proxy, IChannel channel, Output output)
    {
        _proxy = proxy;
        _channel = channel;
        _output = output;
    }

    public void Measure()
    {
        _output.WriteLine("Transaction speed test started");
        _output.WriteLine("Measures client speed for series of echo transaction");
        _output.WriteLine();

        MeasureBandwidth();
        _output.WriteLine();
        MeasureStringBandwidth();
        _output.WriteLine();
        MeasureProtobuffBandwidth();
    }

    public void MeasureBandwidth()
    {
        var test = new TransactionBandwithTest<byte[]>(
            channel: _channel,
            contract: _proxy,
            dataGenerator: Helper.GenerateArray,
            sendProcedure: (iterations, packet) =>
            {
                for (int i = 0; i < iterations; i++)
                    _proxy.AskBytesEcho(packet);
            });

        _output.WriteLine("Bandwidth Test");
        _output.WriteLine("packet [bytes]\t speed [megaBytes per sec]");

        Measure(test, 1, 10000);
        Measure(test, 100, 10000);
        Measure(test, 1000, 10000);
        Measure(test, 60000, 1000);
        Measure(test, 100000, 1000);
        Measure(test, 500000, 500);
        Measure(test, 1000000, 50);
    }

    public void MeasureStringBandwidth()
    {
        var test = new TransactionBandwithTest<string>(
            channel: _channel,
            contract: _proxy,
            dataGenerator: Helper.GenerateString,
            sendProcedure: (iterations, packet) =>
            {
                for (int i = 0; i < iterations; i++)
                    _proxy.AskTextEcho(packet);
            });

        _output.WriteLine("Strting serialization Test");
        _output.WriteLine("packet [chars]\t speed [megaBytes per sec]");

        Measure(test, 1, 100000);
        Measure(test, 100, 10000);
        Measure(test, 1000, 10000);
        Measure(test, 10000, 1000);
        Measure(test, 60000, 1000);
        Measure(test, 100000, 1000);
        Measure(test, 500000,  50);
        Measure(test, 1000000, 50);
    }

     

    public void MeasureProtobuffBandwidth()
    {
        var test = new TransactionBandwithTest<ProtoStruct>(
            channel: _channel,
            contract: _proxy,
            dataGenerator: Helper.GenerateProtoStruct,
            sendProcedure: (iterations, packet) =>
            {
                for (int i = 0; i < iterations; i++)
                    _proxy.AskProtoStructEcho(packet);
            });

        _output.WriteLine("Protobuff serialization Test");
        _output.WriteLine("packet [items]\t speed [megaBytes per sec]");

        Measure(test, 1, 10000);
        Measure(test, 10, 10000);
        Measure(test, 100, 5000);
        Measure(test, 1000, 500);
        Measure(test, 10000, 50);
        Measure(test, 100000, 5);
        Measure(test, 1000000, 1);
    }

       
    void Measure<T>(TransactionBandwithTest<T> test, int items, int iterationsCount)
    {
        var results = test.Test(items, iterationsCount);
        _output.WriteLine(
            $"{items:000000}           \t{results.TotalBandwidthMbs:0.0}");

    }
}