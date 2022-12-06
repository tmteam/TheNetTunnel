using System.Diagnostics;
using System.IO;
using ProtoBuf;
using TNT.SpeedTest.Contracts;

namespace TNT.SpeedTest;

public class ProtobuffNetClearSerialzationTest
{
    private readonly Output _output;

    public ProtobuffNetClearSerialzationTest(Output output)
    {
        _output = output;
    }
    
    public void Run()
    {
        _output.WriteLine("Protobuf-net serialization speed measurement");
        _output.WriteLine("Shows protobuf-net library serialization  speed (megaBYtes per second)\r\n" +
                          " as relative indicator");
        _output.WriteLine();

        _output.WriteLine($"items  \tser  \tdeser");
        Run( 1, 100000);
        Run( 10, 100000);
        Run( 100, 10000);
        Run( 1000, 1000);
        Run( 10000, 100);
        Run( 100000, 10);
        Run( 1000000, 3);
    }
    
    public void Run(int memeberSize, int iterationsCount)
    {
        var packet = Helper.GenerateProtoStruct(memeberSize);
        var serializedLength = 0;
        Serializer.PrepareSerializer<ProtoStruct>();

        Stopwatch serializationSw = new Stopwatch();
        serializationSw.Start();

        //Act:
        for (int i = 0; i < iterationsCount; i++)
        {
            using var stream = new MemoryStream();
            stream.Position = 0;

            ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, packet, PrefixStyle.None);
            serializedLength += (int)stream.Position;
        }
        serializationSw.Stop();
        Stopwatch desserializationSw = new Stopwatch();
        desserializationSw.Start();
        using (var stream = new MemoryStream())
        {
            stream.Position = 0;
            ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, packet, PrefixStyle.None);
            for (int i = 0; i < iterationsCount; i++)
            {
                stream.Position = 0;
                var result = ProtoBuf.Serializer.DeserializeWithLengthPrefix<ProtoStruct>(stream, PrefixStyle.None);
            }
        }
        desserializationSw.Stop();
        var megabytesSerialized = serializedLength / (1024d * 1024d);

        _output.WriteLine(
            $" {memeberSize}\t" +
            $" { (megabytesSerialized / serializationSw.Elapsed.TotalSeconds):0.0}" +
            $"\t {(megabytesSerialized / desserializationSw.Elapsed.TotalSeconds):0.0} ");
    }
}