using TNT.Contract;

namespace TNT.Tests.Contracts
{
    public interface ICallContract
    {
        [ContractMessage(CallContract.AskSummId)] double AskSumm(double a, double b);
        [ContractMessage(CallContract.AskVoidId)] double AskVoid();
        [ContractMessage(CallContract.SayIntStringId)] void SayIntString(int arg1, string arg2);
        [ContractMessage(CallContract.SayVoidId)] void SayVoid();
    }
}