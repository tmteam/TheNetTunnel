namespace TNT.Testing
{
    public class TestChannelPair
    {
        public TestChannelPair(TestChannel cahnnelA, TestChannel channelB)
        {
            CahnnelA = cahnnelA;
            ChannelB = channelB;
            FromAToB = new OneSideConnection(CahnnelA, channelB);
            FromBToA = new OneSideConnection(ChannelB, cahnnelA);
        }

        private OneSideConnection FromAToB;
        private OneSideConnection FromBToA;
        
        public TestChannel CahnnelA { get; }
        public TestChannel ChannelB { get; }

        public bool IsConnected { get; private set; }

        public void Connect()
        {
            CahnnelA.ImmitateConnect();
            ChannelB.ImmitateConnect();
            FromAToB.Start();
            FromBToA.Start();
            IsConnected = true;
        }

        public void ConnectAndStartReceiving()
        {
            Connect();
            ChannelB.AllowReceive = true;
            CahnnelA.AllowReceive = true;
        }

        public void Disconnect()
        {
            FromAToB.Stop();
            FromBToA.Stop();
            IsConnected = false;

        }
    }
}