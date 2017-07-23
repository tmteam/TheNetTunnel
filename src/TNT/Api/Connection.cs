using System;
using TNT.Transport;

namespace TNT.Api
{
    public class Connection<TContract, TChannel>: IDisposable 
        where TChannel: IChannel
    {
        private readonly Action<TContract, IChannel> _onContractDisconnected;

        public Connection(TContract contract, TChannel channel, Action<TContract, IChannel> onContractDisconnected)
        {
            _onContractDisconnected = onContractDisconnected;
            Contract = contract;
            Channel = channel;
            Channel.OnDisconnect += Channel_OnDisconnect;
        }

        private void Channel_OnDisconnect(IChannel obj)
        {
            _onContractDisconnected?.Invoke(Contract, obj);
        }

        public TContract Contract { get; }
        public TChannel Channel { get; }
        public void Dispose()
        {
        }
    }
}