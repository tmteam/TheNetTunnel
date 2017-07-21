using System;
using TNT.Presentation;

namespace TNT.Testing
{
    public class TestChannelListener: IChannelListener<TestChannel>
    {
        public bool IsListening { get; set; }
        public event Action<IChannelListener<TestChannel>, TestChannel> Accepted;

        public TestChannelPair ImmitateAccept(TestChannel incomeChannel)
        {
            if(!IsListening)
                throw  new  InvalidOperationException();
            var thisChannel = new TestChannel();
            var pair = TntTestHelper.CreateChannelPair(thisChannel, incomeChannel);
            pair.Connect();
            Accepted?.Invoke(this, thisChannel);
            return pair;
        }
    }
}
