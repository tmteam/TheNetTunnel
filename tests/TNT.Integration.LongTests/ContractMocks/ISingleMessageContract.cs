using TNT;

namespace Tnt.LongTests.ContractMocks
{
    public interface ISingleMessageContract<TMessageArg>
    {
        [TntMessage(1)] bool Ask(TMessageArg message);
    }
}