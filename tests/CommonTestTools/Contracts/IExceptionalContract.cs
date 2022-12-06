using TNT;

namespace CommonTestTools.Contracts;

public interface IExceptionalContract
{
    [TntMessage(1)] int Ask();
    [TntMessage(2)] void Say();
}