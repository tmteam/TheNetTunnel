using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNT.Presentation;

namespace TNT.Channel.Test
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
