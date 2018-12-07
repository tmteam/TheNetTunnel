using System;
using System.Collections.Generic;

namespace CommonTestTools.Contracts
{
    public class TestContractMock:ITestContract
    {
        public const int AskReturns = 42;

        public int SayCalledCount { get; set; }

        public void Say()
        {
            SayCalledCount++;
        }
        public List<string> SaySCalled { get; } = new List<string>();
        public void Say(string s)
        {
            SaySCalled.Add(s);
            SayMethodWasCalled?.Invoke(this, s);
        }

        public event Action<object, string> SayMethodWasCalled;
        public void Say(string s, int i, long l)
        {
        }

        public int Ask()
        {
            return AskReturns;
        }

        public string Ask(string s)
        {
            return "not implemented";
        }

        private Func<string, int, long, string> _whenAskSILCalled = (s, i, l) => "0";
        public void WhenAskSILCalledCall(Func<string, int, long, string> whenAskSILCalled)
        {
            _whenAskSILCalled = whenAskSILCalled;
        }
        public string Ask(string s, int i, long l)
        {
            return _whenAskSILCalled(s,i,l);
        }

        public Action OnSay { get; set; }
        public Action<string> OnSayS { get; set; }
        public Action<string, int, long> OnSaySIL { get; set; }
        public Func<int> OnAsk { get; set; }
        public Func<string, string> OnAskS { get; set; }
        public Func<string, int, long, string> OnAskSIL { get; set; }
    }
}
