namespace TNT.Channel.Test
{
    interface IOneSideConnection
    {
        TestChannel CahnnelFrom { get; }
        TestChannel ChannelTo { get; }
        void OnSideConnection(TestChannel cahnnelFrom, TestChannel channelTo);
        void Start();
        void Stop();
    }
}