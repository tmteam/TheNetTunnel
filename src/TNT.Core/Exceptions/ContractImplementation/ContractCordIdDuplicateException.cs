using System;

namespace TNT.Exceptions.ContractImplementation;

public class ContractMessageIdDuplicateException : Exception
{
    private readonly int _duplicateId;
    private readonly Type _contractInterfaceType;
    private readonly string _member1Name;
    private readonly string _member2Name;

    public ContractMessageIdDuplicateException(
        int duplicateId, 
        Type contractInterfaceType, 
        string member1Name,
        string member2Name)
        : base(string.Format("Members \"{0}\" and \"{1}\" of contract \"{2}\" has duplicated message id \"{3}\".",
            member1Name, member2Name, contractInterfaceType.Name, duplicateId))
    {
        _duplicateId = duplicateId;
        _contractInterfaceType = contractInterfaceType;
        _member1Name = member1Name;
        _member2Name = member2Name;

    }

    public int DuplicateId => _duplicateId;

    public string Member1Name => _member1Name;

    public string Member2Name => _member2Name;

    public Type ContractInterfaceType => _contractInterfaceType;
}