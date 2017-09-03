using TNT.Contract;

namespace TNT.IntegrationTests.ContractMocks
{
    public interface ISingleMessageContract<TMessageArg>
    {
        [TntMessage(1)] bool Ask(TMessageArg message);
    }
}