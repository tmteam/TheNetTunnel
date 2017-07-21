using TNT.Presentation;

namespace TNT.Testing
{
    public class TestChannelServer<TContract>: ChannelServer<TContract,TestChannel> 
        where TContract : class
    {
        public TestChannelListener TestListener { get;  }
        public TestChannelServer(ConnectionBuilder<TContract> channelBuilder) : base(channelBuilder, new TestChannelListener())
        {
            TestListener = this.Listener as TestChannelListener;
        }
    }
}
