using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Channel.Test
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
