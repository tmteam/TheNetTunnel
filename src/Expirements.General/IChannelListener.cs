using System;
using TNT.Channel;

namespace Expirements.General
{
    public interface IChannelListener<out TChannel> where TChannel : IChannel
    {
        bool IsListening { get; set; }
        event Action<IChannelListener<TChannel>, TChannel> Accepted;
    }
}