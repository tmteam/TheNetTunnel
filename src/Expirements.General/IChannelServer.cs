using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expirements.General
{
   public interface  IChannelServer<TChannel, TContract>
    {
        event Action<IChannelServer<TChannel, TContract>, BeforeConnectEventArgs<TContract, TChannel>>  BeforeConnect;
        event Action<IChannelServer<TChannel, TContract>, ContractContext<TContract, TChannel>> AfterConnect;
        event Action<IChannelServer<TChannel, TContract>, ContractContext<TContract, TChannel>> Disconnected;
	    bool IsListening { get; set; }
        IEnumerable<ContractContext<TContract, TChannel>> GetAllConnections();
    }

    public class BeforeConnectEventArgs<TContract, TChannel> : EventArgs
    {
        public BeforeConnectEventArgs(ContractContext<TContract, TChannel> connection)
        {
            Connection = connection;
            AllowConnection = true;
        }

        public ContractContext<TContract, TChannel> Connection { get; }
        public bool AllowConnection { get; set; }
    }
}
