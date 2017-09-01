using System;
using TNT.Presentation;
using TNT.Transport;

namespace TNT.Api
{

    public class Connection<TContract, TChannel> : IDisposable, IConnection<TContract, TChannel> where TChannel: IChannel
    {
        private readonly Action<TContract, IChannel, ErrorMessage> _onContractDisconnected;

        public Connection(TContract contract, TChannel channel, Action<TContract, IChannel, ErrorMessage> onContractDisconnected)
        {
            _onContractDisconnected = onContractDisconnected;
            Contract = contract;
            Channel = channel;
            Channel.OnDisconnect += Channel_OnDisconnect;
        }

        private void Channel_OnDisconnect(object obj, ErrorMessage cause)
        {
            _onContractDisconnected?.Invoke(Contract, Channel, cause);
        }

        public TContract Contract { get; }
        public TChannel Channel { get; }
        public void Dispose()
        {
            if(Channel.IsConnected)
                Channel.Disconnect();
        }
    }
}