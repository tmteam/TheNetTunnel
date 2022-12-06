using System;

namespace TNT.Exceptions.ContractImplementation;

public class ContractMemberAttributeMissingException: Exception
{
    private readonly Type _contractType;
    private readonly string _memberName;

    public ContractMemberAttributeMissingException(Type contractType, string memberName)
        : base(
            string.Format("Contract \"{0}\" member \"{1}\" has no  message id attribute", contractType.Name,
                memberName))
    {
        _contractType = contractType;
        _memberName = memberName;
    }

    public Type ContractType => _contractType;

    public string MemberName => _memberName;
}