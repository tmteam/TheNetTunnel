using System;
using TNT.Contract;

namespace TNT.SpeedTest.Contracts
{
    public interface ISpeedTestContract
    {
       [ContractMessage(1)]  byte[] AskBytesEcho(byte[] data);
       [ContractMessage(2)]  int[] AskIntegersEcho(int[] data);
       [ContractMessage(3)]  string AskTextEcho(string data);
       [ContractMessage(4)] ProtoStruct AskProtoStructEcho(ProtoStruct data);
       [ContractMessage(5)] void SayNothing();
       [ContractMessage(6)] void SayBytes(byte[] data);
       [ContractMessage(7)] void SayProtoStructEcho(ProtoStruct data);
       [ContractMessage(8)] void SayString(string data);
       [ContractMessage(9)] bool AskForTrue();
       [ContractMessage(10)] void SubscribeForSayCalled(int sayCalldTimes);
       [ContractMessage(11)] Action SaysCallsCountReceived { get; set; }
    }
}