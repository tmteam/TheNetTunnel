using System;

namespace EmitExperiments
{
    [AttributeUsage( AttributeTargets.Method
        | AttributeTargets.Property, AllowMultiple = false, Inherited= false)]
    public class ContractMessageAttribute: Attribute
    {
        private readonly ushort _id;

        public ContractMessageAttribute(ushort id)
        {
            _id = id;
        }

        public ushort Id
        {
            get { return _id; }
        }
    }
}