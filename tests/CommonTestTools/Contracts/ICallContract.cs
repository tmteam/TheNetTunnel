using TNT;

namespace CommonTestTools.Contracts
{
    public interface ICallContract
    {
        [TntMessage(CallContract.AskSummId)] double AskSumm(double a, double b);
        [TntMessage(CallContract.AskVoidId)] double AskVoid();
        [TntMessage(CallContract.SayIntStringId)] void SayIntString(int arg1, string arg2);
        [TntMessage(CallContract.SayVoidId)] void SayVoid();
    }
}