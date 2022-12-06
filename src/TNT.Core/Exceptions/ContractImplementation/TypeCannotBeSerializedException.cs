using System;

namespace TNT.Exceptions.ContractImplementation;

public class TypeCannotBeSerializedException : Exception
{
    private readonly Type _t;

    public TypeCannotBeSerializedException(Type t) :
        base("Type " + t.Name + " cannot be seserialized.")
    {
        _t = t;
    }

    public Type TypeCannotBeSerialized => _t;
}