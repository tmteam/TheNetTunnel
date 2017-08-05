using System;
using System.Threading;

namespace TNT.SpeedTest.Contracts
{
    // Ultimate air heating contract
    public class SpeedTestContract: ISpeedTestContract
    {
        private int _sayCalled = 0;
        private int _sayCalledTimesOffset = int.MaxValue;

        public byte[] AskBytesEcho(byte[] data)
        {
            return data;
        }

        public int[] AskIntegersEcho(int[] data)
        {
            return data;
        }

        public string AskTextEcho(string data)
        {
            return data;
        }

        public ProtoStruct AskProtoStructEcho(ProtoStruct data)
        {
            return data;
        }
        public bool AskForTrue()
        {
            return false;
        }

        public void SayNothing()
        {
            HandleSayCall();
        }

        public void SayBytes(byte[] data)
        {
            HandleSayCall();
        }

        public void SayProtoStructEcho(ProtoStruct data)
        {
            HandleSayCall();
        }
        public void SayString(string data)
        {
            HandleSayCall();
        }
       

        public void SubscribeForSayCalled(int sayCalledTimes)
        {
            _sayCalled = 0;
            _sayCalledTimesOffset = sayCalledTimes;
        }

        public Action SaysCallsCountReceived { get; set; }

        private void HandleSayCall()
        {
            if (Interlocked.Increment(ref _sayCalled) >= _sayCalledTimesOffset)
            {
                _sayCalled = 0;
                SaysCallsCountReceived();
            }
        }

       
    }
}