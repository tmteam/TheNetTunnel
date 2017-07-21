using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TNT.Exceptions.ContractImplementation;

namespace TNT.Contract
{
    public class ContractInfo
    {
        public Type ContractInterfaceType { get; }

        public ContractInfo(Type contractInterfaceType)
        {
            ContractInterfaceType = contractInterfaceType;
        }
        public Dictionary<int, MemberInfo> Memebers { get; } = new Dictionary<int, MemberInfo>();

        public void ThrowIfAlreadyContainsId(int cordId,  MemberInfo memberInfo)
        {
            if (Memebers.ContainsKey(cordId))
                throw new ContractCordIdDuplicateException(
                    cordId,
                    ContractInterfaceType, 
                    Memebers[cordId].Name, 
                    memberInfo.Name);
        }

        public void AddInfo(int cordId, MemberInfo info)
        {
            Memebers.Add(cordId, info);
        }

        public IEnumerable<KeyValuePair<int, PropertyInfo>> GetProperties()
        {
            return
                Memebers
                    .Where(m => m.Value is PropertyInfo)
                    .Select(m => new KeyValuePair<int, PropertyInfo>(m.Key, m.Value as PropertyInfo));
        }
        public IEnumerable<KeyValuePair<int, MethodInfo>> GetMethods()
        {
            return
                Memebers
                    .Where(m => m.Value is MethodInfo)
                    .Select(m => new KeyValuePair<int, MethodInfo>(m.Key, m.Value as MethodInfo));
        }
    }
}
