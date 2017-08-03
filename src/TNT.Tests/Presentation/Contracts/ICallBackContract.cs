using System;
using TNT.Contract;

namespace TNT.Tests.Presentation.Contracts
{
    public interface ICallBackContract
    {
        [ContractMessage(CallBackContract.AskSummId)]
        Func<double, double, double> AskSumm { get; set; }

        [ContractMessage(CallBackContract.AskVoidId)]
        Func<double> AskVoid { get; set; }

        [ContractMessage(CallBackContract.SayIntStringCallBackId)]
        Action<int, string> SayIntString { get; set; }

        [ContractMessage(CallBackContract.SayVoidCallBackId)]
        Action SayVoid { get; set; }
    }
}