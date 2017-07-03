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

        [ContractMessage(84)] string AskSomething(int intParameter, double doubleParameter);

        [ContractMessage(87)] int GiveMe42();
        [ContractMessage(88)] void TheProcedure();

        [ContractMessage(3)] void SaySomething3(int intParameter, string str,  double doubleParameter);
        [ContractMessage(4)] void SaySomething4(int intParameter, string str, double doubleParameter, object lalal, object[] objs);

        [ContractMessage(50)] Action OnProcedureEvent { get; set; }

        [ContractMessage(51)] Action<int, DateTime> OnEvent { get; set; }

        //[ContractMessage(52)] Func<int, DateTime, string> OnAsk { get; set; }

        [ContractMessage(53)] Func<int> GiveMe42Ask { get; set; }
    }


    public interface IOutputCordApi
    {
        void Say(int cordId, object[] values);
        T Ask<T>(int cordId, object[] values);
        void SaySubscribe(int cordId, Action<object[]> callback);
        void AskSubscribe<T>(int cordId, Func<object[], T> callback);
        void Unsubscribe(int cordId);
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

        public T Ask<T>(int cordId, object[] values)
        {
            Console.WriteLine("Ask");
            foreach (var value in values)
            {
                Console.WriteLine("   value: " + value);
            }
            object ans = null;
            if (typeof(T) == typeof(int))
                ans = 42;
            else if (typeof(T) == typeof(string))
                ans = "fortytwo";
            else ans = default(T);

            return (T)ans;
        }

        public void Subscribe(int cordId, Delegate callback)
        {
            

        }
        public Dictionary<int, Action<object[]> > SaySubScribed = new Dictionary<int, Action<object[]>>();
        public void SaySubscribe(int cordId, Action<object[]> callback)
        {
            SaySubScribed.Add(cordId, callback);
        }

        public void AskSubscribe<T>(int cordId, Func<object[], T> callback)
        {
        }

        public void Unsubscribe(int cordId)
        {
        }

        //public void AskSubscribe<T>(int cordId, Func<object[], object> callBack)
        //{
        //    Func<object[], T> callbackT = callBack.;
        //}
    }
}
