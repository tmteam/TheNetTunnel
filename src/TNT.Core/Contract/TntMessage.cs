using System;

// ReSharper disable once CheckNamespace
namespace TNT;

[AttributeUsage( AttributeTargets.Method
                 | AttributeTargets.Property, AllowMultiple = false, Inherited= true)]
public class TntMessage: Attribute
{
    private readonly ushort _id;

    public TntMessage(ushort id)
    {
        _id = id;
    }

    public ushort Id => _id;
}