using TNT.Presentation;

namespace TNT.Tests.Presentation.Contracts
{
    public interface IExceptionalContract
    {
        [ContractMessage(1)] int Ask();
        [ContractMessage(2)] void Say();
    }
}