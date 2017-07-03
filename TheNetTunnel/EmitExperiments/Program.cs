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
            contract.SaySomething2(39,  12.5);
            contract.SaySomething3(39, "123", 12.5);
            contract.SaySomething4(39, "123", 12.5, DateTime.Now, new object[] {1,DateTime.Now, "asd"});

            var res = contract.AskSomething(38, 55);
            Console.WriteLine("Result is: "+ res);

            var res2 = contract.GiveMe42();
            Console.WriteLine("Result2 is: " + res2);

            contract.TheProcedure();
            apiMock.SaySubScribed[50](new object[0]);

            contract.OnProcedureEvent += () => { Console.WriteLine("ProcedureIsCalled"); };
            apiMock.SaySubScribed[50](new object[0]);

            apiMock.SaySubScribed[51](new object[] {123, DateTime.Now});
            contract.OnEvent += (i, dt) => { Console.WriteLine("ActionIsCalled "+ i +" "+ dt); };

            apiMock.SaySubScribed[51](new object[] { 123, DateTime.Now });

            Console.ReadLine();
        }
    }
}
