using System;
using TNT.Contract;
using TNT.Presentation;

namespace TNT.Tests.Presentation.Origin.OriginContracts
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