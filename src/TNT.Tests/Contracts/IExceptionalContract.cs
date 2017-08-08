using TNT.Contract;

namespace TNT.Tests.Contracts
{
    public interface IExceptionalContract
    {
        [ContractMessage(1)] int Ask();
        [ContractMessage(2)] void Say();
    }
}