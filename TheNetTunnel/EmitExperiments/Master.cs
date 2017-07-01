using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TheTunnel.Deserialization;
using TheTunnel.Serialization;

namespace EmitExperiments
{
    public class MasterBulder
    {
        public MasterBulder UseContractHandler(object contract)
        {
            return this;
        }

        public MasterBulder UseProxyContract<T>()
        {
            return this;
        }

        public MasterBulder UseCustomSerializers(params ISerializer[] serializers)
        {
            return this;
        }
        public MasterBulder UseCustomDeserializers(params IDeserializer[] deserializers)
        {
            return this;
        }
        public MasterBulder UseEndpoint(IPEndPoint endpoint)
        {
            return this;
        }

        public Master Build()
        {
            return  new Master();
        }
        public Master BuildFor(object contractHanler)
        {
            return new Master();
        }

        public Master<TProxyContract> BuildFor<TProxyContract>()
        {
            return new Master<TProxyContract>();
        }
    }

    public class Master<TProxyContract> : Master
    {
        public TProxyContract Contract { get; private set; }
    }
    public class Master
    {
        public void Connect()
        {
            throw new NotImplementedException();
            //return  new SayingContract();
        }
    }
}
