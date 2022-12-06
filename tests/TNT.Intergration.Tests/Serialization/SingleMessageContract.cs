using System;

namespace TNT.IntegrationTests.Serialization;

public class SingleMessageContract<TMessageArg> : ISingleMessageContract<TMessageArg>
{
    public Action<object,TMessageArg> SayCalled { get; set; }
    public bool Ask(TMessageArg message)
    {
        SayCalled?.Invoke(this,message);
        return true;
    }
}