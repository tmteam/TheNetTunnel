using System;
using System.Collections.Generic;

namespace TNT.Channel
{
   public interface  IChannelServer<TContract,TChannel> where TChannel : IChannel
    {
        event Action<IChannelServer<TContract,        TChannel>, BeforeConnectEventArgs<TContract, TChannel>>  BeforeConnect;
        event Action<IChannelServer<TContract,        TChannel>, Connection<TContract, TChannel>> AfterConnect;
        event Action<IChannelServer<TContract,        TChannel>, Connection<TContract, TChannel>> Disconnected;
	    bool IsListening { get; set; }
        IEnumerable<Connection<TContract, TChannel>> GetAllConnections();
        void Close();
    }

    public class BeforeConnectEventArgs<TContract, TChannel> : EventArgs where TChannel: IChannel
    {
        public BeforeConnectEventArgs(Connection<TContract, TChannel> connection)
        {
            Connection = connection;
            AllowConnection = true;
        }

        public Connection<TContract, TChannel> Connection { get; }
        public bool AllowConnection { get; set; }
    }
}
