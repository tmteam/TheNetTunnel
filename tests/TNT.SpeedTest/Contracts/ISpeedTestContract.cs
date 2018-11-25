

using System;

namespace TNT.SpeedTest.Contracts
{
    public interface ISpeedTestContract
    {
       [TntMessage(1)]  byte[] AskBytesEcho(byte[] data);
       [TntMessage(2)]  int[] AskIntegersEcho(int[] data);
       [TntMessage(3)]  string AskTextEcho(string data);
       [TntMessage(4)] ProtoStruct AskProtoStructEcho(ProtoStruct data);
       [TntMessage(5)] void SayNothing();
       [TntMessage(6)] void SayBytes(byte[] data);
       [TntMessage(7)] void SayProtoStructEcho(ProtoStruct data);
       [TntMessage(8)] void SayString(string data);
       [TntMessage(9)] bool AskForTrue();
       [TntMessage(10)] void SubscribeForSayCalled(int sayCalldTimes);
       [TntMessage(11)] Action SaysCallsCountReceived { get; set; }
    }
}