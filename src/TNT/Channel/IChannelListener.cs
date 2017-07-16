using System;

namespace TNT.Channel
{
    public interface IChannelListener<out TChannel> where TChannel : IChannel
    {
        bool IsListening { get; set; }
        event Action<IChannelListener<TChannel>, TChannel> Accepted;
    }
}