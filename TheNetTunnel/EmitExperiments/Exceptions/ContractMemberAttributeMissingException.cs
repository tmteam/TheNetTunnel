using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitExperiments
{
    public class ContractMemberAttributeMissingException: Exception
    {
        private readonly Type _contractType;
        private readonly string _memberName;

        public ContractMemberAttributeMissingException(Type contractType, string memberName)
            : base(
                string.Format("Contract \"{0}\" member \"{1}\" has no cord message id attribute", contractType.Name,
                    memberName))
        {
            _contractType = contractType;
            _memberName = memberName;
        }

        public Type ContractType
        {
            get { return _contractType; }
        }

        public string MemberName
        {
            get { return _memberName; }
        }
    }
}
