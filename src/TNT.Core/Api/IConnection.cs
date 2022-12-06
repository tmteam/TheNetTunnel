using System;
using TNT.Transport;

namespace TNT.Api;

public interface IConnection<out TContract, out TChannel> : IDisposable
    where TChannel : IChannel
{
    TChannel Channel { get; }
    
    TContract Contract { get; }

    void Dispose();
}