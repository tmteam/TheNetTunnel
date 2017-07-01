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
        [ContractMessage(3)] void SaySomething3(int intParameter, string str,  double doubleParameter);
        [ContractMessage(4)] void SaySomething4(int intParameter, string str, double doubleParameter, object lalal, object[] objs);
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

    public class OutputCordApi : IOutputCordApi
    {
        public void Say(int cordId, object[] values)
        {
            Console.WriteLine("Say. Arg.Count: "+ values.Length);
            foreach (var value in values)
            {
                Console.WriteLine("   value: "+value);
            }
        }

        public object Ask(int cordId, object[] values)
        {
            Console.WriteLine("Ask");
            foreach (var value in values)
            {
                Console.WriteLine("   value: " + value);
            }
            return null;
        }
    }
}
