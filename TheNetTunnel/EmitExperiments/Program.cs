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
    class Program
    {
        static void Main(string[] args)
        {
            //var master = new MasterBulder()
            //    .UseCustomSerializers(new ArraySerializer<int>(), new ProtoSerializer<int>())
            //    .UseCustomDeserializers(new ArrayDeserializer<string>(), new ProtoDeserializer<int>())
            //    .UseEndpoint(new IPEndPoint(IPAddress.Any, 17171))
            //    .BuildFor<ISayingContract>();

            //master.Contract.OnNewMessageWithCallBack += (s, time) => 42;
            //master.Contract.OnNewMessage += (s, time, arg3) => Console.WriteLine("got " + time.ToString());

            //master.Connect();

            OutputCordApi apiMock = new OutputCordApi();
            var contract =  ProxyContractFactory.CreateProxyContract<ISayingContract>(apiMock);

            contract.SaySomething(42);
            //Console.WriteLine(answer);

        }
    }
}
