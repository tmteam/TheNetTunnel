namespace TNT.Testing;

public class TestChannelPair
{
    public TestChannelPair(TestChannel channelA, TestChannel channelB)
    {
        ChannelA = channelA;
        ChannelB = channelB;
        _fromAToB = new OneSideConnection(ChannelA, channelB);
        _fromBToA = new OneSideConnection(ChannelB, channelA);
    }

    private readonly OneSideConnection _fromAToB;
    private readonly OneSideConnection _fromBToA;
        
    public TestChannel ChannelA { get; }
    public TestChannel ChannelB { get; }

    public bool IsConnected { get; private set; }

    public void Connect()
    {
        ChannelA.ImmitateConnect();
        ChannelB.ImmitateConnect();
        _fromAToB.Start();
        _fromBToA.Start();
        IsConnected = true;
    }

    public void ConnectAndStartReceiving()
    {
        Connect();
        ChannelB.AllowReceive = true;
        ChannelA.AllowReceive = true;
    }

    public void Disconnect()
    {
        _fromAToB.Stop();
        _fromBToA.Stop();
        ChannelB.ImmitateDisconnect();
        ChannelA.ImmitateDisconnect();
        IsConnected = false;

    }
}