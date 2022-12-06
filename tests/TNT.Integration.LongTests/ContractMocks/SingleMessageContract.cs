using System;

namespace Tnt.LongTests.ContractMocks;

public class SingleMessageContract<TMessageArg> : ISingleMessageContract<TMessageArg>
{
    public Action<object,TMessageArg> SayCalled { get; set; }
    public bool Ask(TMessageArg message)
    {
        SayCalled?.Invoke(this,message);
        return true;
    }
}