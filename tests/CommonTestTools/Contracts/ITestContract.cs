using System;
using TNT;

namespace CommonTestTools.Contracts
{
    public interface ITestContract
    {
        [TntMessage(1)]
        void Say();
        [TntMessage(2)]
        void Say(string s);
        [TntMessage(3)]
        void Say(string s, int i, long l);
        [TntMessage(4)]
        int Ask();
        [TntMessage(5)]
        string Ask(string s);
        [TntMessage(6)]
        string Ask(string s, int i, long l);

        [TntMessage(101)]
        Action OnSay { get; set; }
        [TntMessage(102)]
        Action<string> OnSayS { get; set; }
        [TntMessage(103)]
        Action<string, int, long> OnSaySIL { get; set; }
        [TntMessage(104)]
        Func<int> OnAsk { get; set; }
        [TntMessage(105)]
        Func<string, string> OnAskS { get; set; }
        [TntMessage(106)]
        Func<string,int, long, string> OnAskSIL { get; set; }
    }
}
