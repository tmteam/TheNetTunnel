using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheTunnel.Cords;
using TheTunnel.Serialization;

namespace EmitExperiments
{
    public interface ISayingContract
    {
        [ContractMessage(1)] void SaySomething(int intParameter);
        [ContractMessage(2)] void SaySomething2(int intParameter, double doubleParameter);

        //[ContractMessage(3)] string AskSomething(int intParameter, double doubleParameter);

        //[ContractMessage(4)] Action<string,DateTime,ushort> OnNewMessage { get; set; }
        //[ContractMessage(5)] Func<string, DateTime, ushort> OnNewMessageWithCallBack { get; set; }

    }


    public interface ICordMessageHandler
    {
        Delegate Handle(ushort tellCordId);
    }


    public interface IOutputCordApi
    {
        void Say(int cordId, object[] values);
        object Ask(int cordId, object[] values);
       
    }

    class OutputCordApi : IOutputCordApi
    {
        public void Say(int cordId, object[] values)
        {
            Console.WriteLine("Say");
        }

        public object Ask(int cordId, object[] values)
        {
            Console.WriteLine("Ask");
            return null;
        }
    }
}
