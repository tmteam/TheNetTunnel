using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNT.LocalSpeedTest.Contracts;
using TNT.Transport;

namespace TNT.LocalSpeedTest.OutputBandwidth
{
    public class OutputTestMeasurement
    {
        private readonly ISpeedTestContract _proxy;
        private readonly IChannel _channel;
        private readonly Output _output;

        public OutputTestMeasurement(ISpeedTestContract proxy, IChannel channel, Output output)
        {
            _proxy = proxy;
            _channel = channel;
            _output = output;
        }

        public void Measure()
        {
            _output.WriteLine("Output speed test started");
            TestBandwidth();
            _output.WriteLine();
            TestStringBandwidth();
            _output.WriteLine();
            TestProtobuffBandwidth();
        }

        public void TestBandwidth()
        {
            var test = new OutputBandwithTest<byte[]>(
                channel: _channel,
                contract: _proxy,
                dataGenerator: Helper.GenerateArray,
                sendProcedure: (iterations, packet) =>
                {
                    for (int i = 0; i < iterations; i++)
                        _proxy.SayBytes(packet);
                });

            _output.WriteLine("Bandwidth Test");
            _output.WriteLine("Packet [bytes]"+ OutputBandwithTestResults.GetTabbedHeader());

            MeasureBandWidth(test, 1, 100000);
            MeasureBandWidth(test, 100, 10000);
            MeasureBandWidth(test, 1000, 10000);
            MeasureBandWidth(test, 60000, 1000);
            MeasureBandWidth(test, 100000, 1000);
            MeasureBandWidth(test, 500000, 500);
            MeasureBandWidth(test, 1000000, 50);
        }

        public void TestStringBandwidth()
        {
            var test = new OutputBandwithTest<string>(
                channel: _channel,
                contract: _proxy,
                dataGenerator: Helper.GenerateString,
                sendProcedure: (iterations, packet) =>
                {
                    for (int i = 0; i < iterations; i++)
                        _proxy.SayString(packet);
                });

            _output.WriteLine("Strting serialization Test");
            _output.WriteLine("Packet [chars]" + OutputBandwithTestResults.GetTabbedHeader());

            MeasureBandWidth(test, 1,   10000);
            MeasureBandWidth(test, 100, 10000);
            MeasureBandWidth(test, 1000, 10000);
            MeasureBandWidth(test, 10000, 1000);
            MeasureBandWidth(test, 60000, 1000);
            MeasureBandWidth(test, 100000, 1000);
            MeasureBandWidth(test, 500000,  50);
            MeasureBandWidth(test, 1000000, 10);
        }


        public void TestProtobuffBandwidth()
        {
            var test = new OutputBandwithTest<ProtoStruct>(
                channel: _channel,
                contract: _proxy,
                dataGenerator: Helper.GenerateProtoStruct,
                sendProcedure: (iterations, packet) =>
                {
                    for (int i = 0; i < iterations; i++)
                        _proxy.SayProtoStructEcho(packet);
                });

            _output.WriteLine("Protobuff serialization Test");
            _output.WriteLine("Packet [items]" + OutputBandwithTestResults.GetTabbedHeader());

            MeasureBandWidth(test, 1, 100000);
            MeasureBandWidth(test, 10, 100000);
            MeasureBandWidth(test, 100, 10000);
            MeasureBandWidth(test, 1000, 1000);
            MeasureBandWidth(test, 10000, 100);
            MeasureBandWidth(test, 100000, 10);
            MeasureBandWidth(test, 1000000, 3);
        }
        void MeasureBandWidth<T>(OutputBandwithTest<T> test, int items, int iterationsCount)
        {
            var results = test.Test(items, iterationsCount);
            _output.WriteLine(
                $"{items:000000}      " + results.GetTabbedResults());
        }
    }
}
