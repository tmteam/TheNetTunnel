using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNT.Channel.Tcp;

namespace TNT.Channel.Test
{
    public static class TntTestHelper
    {
        public static TestChannelPair CreateChannelPair()
        {
            return  new TestChannelPair(new TestChannel(), new TestChannel());
        }

        public static TestChannelPair CreateChannelPair(TestChannel cahnnelA, TestChannel channelB)
        {
            return  new TestChannelPair(cahnnelA, channelB);
        }
    }
}