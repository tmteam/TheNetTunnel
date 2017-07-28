using System;
using System.Collections.Generic;
using TNT.Presentation;
using TNT.Transport;

namespace TNT.Api
{
   public interface  IChannelServer<TContract,TChannel> where TChannel : IChannel
    {
        event Action<object, BeforeConnectEventArgs<TContract, TChannel>>  BeforeConnect;
        event Action<object, IConnection<TContract, TChannel>> AfterConnect;
        event Action<object, IConnection<TContract, TChannel>, ErrorMessage> Disconnected;
	    bool IsListening { get; set; }
        IEnumerable<IConnection<TContract, TChannel>> GetAllConnections();
        void Close();
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
}
