using System;
using TNT;

namespace CommonTestTools.Contracts
{
    public interface ICallBackContract
    {
        [TntMessage(CallBackContract.AskSummId)]
        Func<double, double, double> AskSumm { get; set; }

        [TntMessage(CallBackContract.AskVoidId)]
        Func<double> AskVoid { get; set; }

        [TntMessage(CallBackContract.SayIntStringCallBackId)]
        Action<int, string> SayIntString { get; set; }

        [TntMessage(CallBackContract.SayVoidCallBackId)]
        Action SayVoid { get; set; }
    }
}