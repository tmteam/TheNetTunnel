using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TNT.Exceptions;
using TNT.Presentation.Proxy;

namespace TNT.Presentation.Origin
{
    public static class OriginContractLinker
    {
        public static ContractsMemberInfo Link<TInterface>(TInterface contract, ICordInterlocutor interlocutor)
        {
            var contractType = contract.GetType();
            var interfaceType = typeof(TInterface);
            ContractsMemberInfo contractMemebers = GetContractMemebers(contractType, interfaceType);
            foreach (var method in contractMemebers.GetMehodInfos())
            {
                if (method.Value.ReturnParameter.ParameterType == typeof(void))
                {
                    //Say handler method:
                    interlocutor.SaySubscribe(method.Key, (args) => method.Value.Invoke(contract, args));
                }
                else
                {
                    //Ask handler method:
                    interlocutor.AskSubscribe(method.Key, (args) => method.Value.Invoke(contract, args));
                }
            }
            OriginCallbackDelegatesHandlerFactory.CreateFor(contractMemebers, contract, interlocutor);
            return contractMemebers;
        }

        public static ContractsMemberInfo GetContractMemebers(Type contractType, Type interfaceType)
        {
            var contractMemebers = new ContractsMemberInfo(interfaceType);
            foreach (var meth in interfaceType.GetMethods())
            {
                if (meth.IsSpecialName)
                    continue;

                var overrided = ReflectionHelper.GetOverridedMethodOrNull(contractType, meth);

                if (overrided == null)
                    continue;

                var attribute = Attribute.GetCustomAttribute(meth,
                    typeof(ContractMessageAttribute)) as ContractMessageAttribute;
                if (attribute == null)
                    throw new ContractMemberAttributeMissingException(interfaceType, meth.Name);

                contractMemebers.ThrowIfAlreadyContainsId(attribute.Id, overrided);
                contractMemebers.AddInfo(attribute.Id, overrided);
            }
            foreach (var propertyInfo in interfaceType.GetProperties())
            {
                var attribute = Attribute.GetCustomAttribute(
                    propertyInfo,
                    typeof(ContractMessageAttribute)) as ContractMessageAttribute;

                if (attribute == null)
                    throw new ContractMemberAttributeMissingException(interfaceType, propertyInfo.Name);

                var overrided = ReflectionHelper.GetOverridedPropertyOrNull(contractType, propertyInfo);
                if (overrided == null)
                    continue;

                contractMemebers.ThrowIfAlreadyContainsId(attribute.Id, overrided);
                contractMemebers.AddInfo(attribute.Id, overrided);
            }

            return contractMemebers;
        }
    }
}
