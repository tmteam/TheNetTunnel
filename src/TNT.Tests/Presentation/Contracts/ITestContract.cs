using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNT.Contract;
using TNT.Presentation;

namespace TNT.Tests.Presentation.FullStack
{
    public interface ITestContract
    {
        [ContractMessage(1)]
        void Say();
        [ContractMessage(2)]
        void Say(string s);
        [ContractMessage(3)]
        void Say(string s, int i, long l);
        [ContractMessage(4)]
        int Ask();
        [ContractMessage(5)]
        string Ask(string s);
        [ContractMessage(6)]
        int Ask(string s, int i, long l);



        [ContractMessage(101)]
        Action OnSay { get; set; }
        [ContractMessage(102)]
        Action<string> OnSayS { get; set; }
        [ContractMessage(103)]
        Action<string, int, long> OnSaySIL { get; set; }
        [ContractMessage(104)]
        Func<int> OnAsk { get; set; }
        [ContractMessage(105)]
        Func<string, string> OnAskS { get; set; }
        [ContractMessage(106)]
        Func<string,int, long, string> OnAskSIL { get; set; }
    }
}
