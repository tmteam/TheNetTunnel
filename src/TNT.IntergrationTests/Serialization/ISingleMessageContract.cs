using TNT.Contract;

namespace TNT.IntegrationTests.Serialization
{
    public interface ISingleMessageContract<TMessageArg>
    {
        [ContractMessage(1)] bool Ask(TMessageArg message);
    }
}