using System;

namespace TNT.Tests.Contracts
{

    public class CallBackContract : ICallBackContract
    {
        public const int SayIntStringCallBackId = 314;
        public const int SayVoidCallBackId = 007;

        public const int AskSummId = 217;
        public const int AskVoidId = 273;

        public Func<double> AskVoid { get; set; }

        public Func<double, double, double> AskSumm { get; set; }

        public Action<int, string> SayIntString { get; set; }
        public Action SayVoid { get; set; }
    }
}
