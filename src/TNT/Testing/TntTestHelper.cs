namespace TNT.Testing
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