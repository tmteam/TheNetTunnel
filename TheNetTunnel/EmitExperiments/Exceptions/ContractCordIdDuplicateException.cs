using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitExperiments
{
    public class ContractCordIdDuplicateException : Exception
    {
        private readonly int _duplicateId;
        private readonly Type _contractInterfaceType;
        private readonly string _member1Name;
        private readonly string _member2Name;

        public ContractCordIdDuplicateException(
            int duplicateId, 
            Type contractInterfaceType, 
            string member1Name,
            string member2Name)
            : base(string.Format("Members \"{0}\" and \"{1}\" of contract \"{2}\" has duplicated cord id \"{3}\".",
                member1Name, member2Name, contractInterfaceType.Name, duplicateId))
        {
            _duplicateId = duplicateId;
            _contractInterfaceType = contractInterfaceType;
            _member1Name = member1Name;
            _member2Name = member2Name;

        }

        public int DuplicateId
        {
            get { return _duplicateId; }
        }

        public string Member1Name
        {
            get { return _member1Name; }
        }

        public string Member2Name
        {
            get { return _member2Name; }
        }

        public Type ContractInterfaceType
        {
            get { return _contractInterfaceType; }
        }
    }
}
