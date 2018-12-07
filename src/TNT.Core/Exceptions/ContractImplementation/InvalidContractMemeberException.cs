using System;
using System.Reflection;

namespace TNT.Exceptions.ContractImplementation
{
    public class InvalidContractMemeberException : Exception
    {
        private readonly MemberInfo _member;
        private readonly Type _contractType;

        public InvalidContractMemeberException(MemberInfo member, Type contractType)
            : base(
                string.Format(
                    "{0} Contract member \"{1}\" with name \"{2}\" can not be implemented via TNT. Use delegate properties and methods only",
                    contractType.Name, member, member.Name))
        {
            _member = member;
            _contractType = contractType;
        }

        public MemberInfo Member
        {
            get { return _member; }
        }

        public Type ContractType
        {
            get { return _contractType; }
        }
    }
}
