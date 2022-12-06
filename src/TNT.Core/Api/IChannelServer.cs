using System;
using System.Collections.Generic;
using TNT.Presentation;
using TNT.Transport;

namespace TNT.Api;

public interface  IChannelServer<TContract,TChannel> where TChannel : IChannel
{
    event Action<object, BeforeConnectEventArgs<TContract, TChannel>>  BeforeConnect;
    event Action<object, IConnection<TContract, TChannel>> AfterConnect;
    event Action<object, ClientDisconnectEventArgs<TContract, TChannel>> Disconnected;
    int ConnectionsCount { get; }
    void StartListening();
    void StopListening();
    bool IsListening { get; }
    IEnumerable<IConnection<TContract, TChannel>> GetAllConnections();
    void Close();
}

public class ClientDisconnectEventArgs<TContract, TChannel>: EventArgs where TChannel : IChannel
{
    public ClientDisconnectEventArgs(IConnection<TContract, TChannel> connection, ErrorMessage errorMessageOrNull)
    {
        Connection = connection;
        ErrorMessageOrNull = errorMessageOrNull;
    }

    public IConnection<TContract, TChannel> Connection { get; }
    public ErrorMessage ErrorMessageOrNull { get; }
}
public class BeforeConnectEventArgs<TContract, TChannel> : EventArgs where TChannel: IChannel
{
    public BeforeConnectEventArgs(IConnection<TContract, TChannel> connection)
    {
        Connection = connection;
        AllowConnection = true;
    }

    public IConnection<TContract, TChannel> Connection { get; }
    public bool AllowConnection { get; set; }
}