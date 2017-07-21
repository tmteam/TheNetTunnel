using System;
using TNT.Light;

namespace TNT.Presentation
{
    public interface IChannelListener<out TChannel> where TChannel : IChannel
    {
        bool IsListening { get; set; }
        event Action<IChannelListener<TChannel>, TChannel> Accepted;
    }
}