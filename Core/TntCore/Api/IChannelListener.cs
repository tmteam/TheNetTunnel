using System;
using TNT.Transport;

namespace TNT.Api
{
    public interface IChannelListener<out TChannel> where TChannel : IChannel
    {
        bool IsListening { get; set; }
        event Action<IChannelListener<TChannel>, TChannel> Accepted;
    }
}