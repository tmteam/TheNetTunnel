using System;
using TNT.Channel;

namespace TNT.Presentation
{
    public class Connection<TContract, TChannel>: IDisposable 
        where TChannel: IChannel
    {
        public Connection(TContract contract, TChannel channel)
        {
            Contract = contract;
            Channel = channel;
        }

        public TContract Contract { get; }
        public TChannel Channel { get; }
        public void Dispose()
        {
        }
    }
}