using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using TNT.Api;
using TNT.Exceptions.ContractImplementation;
using TNT.Presentation;

namespace TNT.Contract.Origin
{
    public static class OriginCallbackDelegatesHandlerFactory
    {
        /*
         * Generates the Type with following code:
         * 
         * public class CallbackDelegatesHandler_123
         * {
         *     private readonly IInterlocutor _interlocutor;
         * 
         *     public CallbackDelegatesHandler_123(IInterlocutor interlocutor)
         *     {
         *         _interlocutor = interlocutor;
         *     }
         * 
         *     //Handler for some say delegate property with id of 42
         *     public void SomeSayDelegateHandler(int arg1, string arg2)
         *     {
         *         _interlocutor.Say(42,new object[] { arg1, arg2 });
         *     }
         * 
         *     //Handler for some ask delegate property with id of 43
         *     public string SomeAskDelegateHandler(DateTime arg1, string arg2)
         *     {
         *         return _interlocutor.Ask<string>(43, new object[] { arg1, arg2 });
         *     }
         * }
         * 
         */
        private static int _exemmplarCounter;

        public static void CreateFor(ContractInfo contractMembers, object contractObject,
            IInterlocutor interlocutor)
        {
            Dictionary<PropertyInfo, string> delegateToMethodsMap;
            Type type;
            CreateHandlerType(contractMembers, out delegateToMethodsMap, out type);

            var delegateHandler = Activator.CreateInstance(type, interlocutor);

            //Set handlers for origin contract delegate properties:
            foreach (var method in delegateToMethodsMap)
            {
                var del =  delegateHandler
                                .GetType()
                                .GetMethod(method.Value)
                                .CreateDelegate(method.Key.PropertyType, delegateHandler);
                method.Key.SetValue(contractObject, del);
            }
        }

        public static void CreateHandlerType(ContractInfo contractMembers, out Dictionary<PropertyInfo, string> delegateToMethodsMap, out Type generatedType)
        {
            var interlocutorType = typeof(IInterlocutor);
            var typeCount = Interlocked.Increment(ref _exemmplarCounter);

            var typeBuilder =  EmitHelper.CreateTypeBuilder(contractMembers.ContractInterfaceType.Name + "_" + typeCount);
            delegateToMethodsMap = new Dictionary<PropertyInfo, string>();

            const string interlocutorFieldName = "_interlocutor";
            var outputApiFieldInfo = typeBuilder.DefineField(
                                        interlocutorFieldName,
                                        typeof(IInterlocutor),
                                        FieldAttributes.Private);

            EmitHelper.ImplementPublicConstructor(typeBuilder,new[] { outputApiFieldInfo });
            
            var sayMehodInfo = interlocutorType.GetMethod("Say", new[] { typeof(int), typeof(object[]) });

            foreach (var property in contractMembers.GetProperties())
            {
                MethodBuilder metbuilder;

                #region generates delegate handlerMethod  {property.Value.Name}DelegateHandler

                var delegateInfo = ReflectionHelper.GetDelegateInfoOrNull(property.Value.PropertyType);
                    if (delegateInfo == null)
                        //the property is not an delegate
                        throw new InvalidContractMemeberException(property.Value, contractMembers.ContractInterfaceType);

                Type[] parameterTypes = delegateInfo.ParameterTypes;
                Type returnType = delegateInfo.ReturnType;


                if (!parameterTypes.Any() && returnType == typeof(void))
                {
                    metbuilder = typeBuilder.DefineMethod(property.Value.Name + "DelegateHandler",
                        MethodAttributes.Public);
                }
                else
                {
                    metbuilder = typeBuilder.DefineMethod(property.Value.Name + "DelegateHandler",
                        MethodAttributes.Public, returnType, parameterTypes);
                }

                MethodInfo askOrSayMethodInfo = null;
                if (returnType == typeof(void))
                {
                    askOrSayMethodInfo = sayMehodInfo;
                }
                else
                {
                    askOrSayMethodInfo = interlocutorType
                     .GetMethod("Ask", new[] { typeof(int), typeof(object[]) })
                     .MakeGenericMethod(returnType);
                }
                EmitHelper.GenerateSayOrAskMethodBody(
                    messageTypeId: property.Key,
                    interlocutorSayOrAskMethodInfo: askOrSayMethodInfo,
                    interlocutorFieldInfo: outputApiFieldInfo,
                    methodBuilder: metbuilder,
                    callParameters: parameterTypes);
                #endregion

                delegateToMethodsMap.Add(property.Value, metbuilder.Name);
            }
            generatedType = typeBuilder.CreateTypeInfo().AsType();
        }
    }























}
