using System;
using System.Collections.Generic;
using TNT.Presentation;
using TNT.Tcp;

namespace TNT.Tests.Presentation
{
    public class CordInterlocutorMock : ICordInterlocutor
    {
        public const int AskMessage1Id                = 400;
        public const int AskSomethingWithArrayId      = 800;
        public const int SaySomething1Id              = 1000;
        public const int SaySomethingWithArrayId      = 2000;

        public const int ProcedureCallId              = 101;
        public const int ArgumentProcedureCallId      = 102;
        public const int ArrayArgumentProcedureCallId = 103;
        public const int FuncCallReturnsIntId         = 104;
        public const int FuncCallReturnsBoolId        = 109;
        public const int FuncCallReturnsRefTypeId     = 108;
        public const int ArgumentFuncCallId           = 105;
        public const int ArrayArgumentFuncCallId      = 106;
        public const int FuncReturnsArrayCallId       = 107;


        public List<SayOrAskCall> Calls          = new List<SayOrAskCall>();
        public Dictionary<int, object> AskAnwers = new Dictionary<int, object>();
        private Dictionary<int, Action<object[]>> subscribedSay = new Dictionary<int, Action<object[]>>();
        private Dictionary<object, Func<object[],object>> subscribedAsk = new Dictionary<object, Func<object[], object>>();

        public void Say(int cordId, object[] values)
        {
            Calls.Add(new SayOrAskCall {Arguments = values, CordId = cordId});
        }

        public void SetAskAnswer<T>(int cordId, T value)
        {
            if (AskAnwers.ContainsKey(cordId))
                AskAnwers[cordId] = value;
            else
            {
                AskAnwers.Add(cordId, value);
            }
        }

        public T Ask<T>(int cordId, object[] values)
        {
            Calls.Add(new SayOrAskCall {Arguments = values, CordId = cordId});
            if (AskAnwers.ContainsKey(cordId))
                return (T) AskAnwers[cordId];
            else
            {
                return default(T);
            }
        }

        public void Raise(int CordId, params object[] values)
        {
            subscribedSay[CordId](values);
        }

        public T Raise<T>(int CordId, params object[] values)
        {
            return (T)subscribedAsk[CordId](values);
        }


        public void SaySubscribe(int cordId, Action<object[]> callback)
        {
            subscribedSay.Add(cordId,callback);
        }

        public void AskSubscribe<T>(int cordId, Func<object[], T> callback)
        {
            subscribedAsk.Add(cordId, (arg)=>callback(arg));

        }

        public void Unsubscribe(int cordId)
        {
            throw new NotImplementedException();
        }

        public class SayOrAskCall
        {
            public int CordId { get; set; }
            public object[] Arguments { get; set; }
        }
    }
}
