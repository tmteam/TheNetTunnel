using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using TNT.Exceptions.ContractImplementation;
using TNT.Presentation;

namespace TNT.Contract.Proxy
{
    public static class ProxyContractFactory
    {
        private static int _exemmplarCounter;

        public static T CreateProxyContract<T>(ICordInterlocutor interlocutor)
        {
            var interfaceType = typeof(T);
            TypeBuilder typeBuilder =  CreateProxyTypeBuilder<T>();

            typeBuilder.AddInterfaceImplementation(interfaceType);

            const string interlocutorFieldName = "_interlocutor";
            var outputApiFieldInfo = typeBuilder.DefineField(
                                        interlocutorFieldName, 
                                        typeof(ICordInterlocutor),
                                        FieldAttributes.Private);
            
            var sayMehodInfo = interlocutor.GetType().GetMethod("Say", new[] {typeof(int), typeof(object[])});

            var contractMemebers = ParseContractInterface(typeof(T));// new ContractsMemberInfo(typeof(T));

            foreach (var eventInfo in interfaceType.GetEvents())
                throw new InvalidContractMemeberException(eventInfo, interfaceType);


            #region interface methods implementation

            foreach (var method in contractMemebers.GetMethods())
            {
                var methodBuilder = EmitHelper.ImplementInterfaceMethod(method.Value, typeBuilder);

                var returnType = method.Value.ReturnType;
                MethodInfo askOrSayMethodInfo = null;
                if (returnType == typeof(void))
                {
                    askOrSayMethodInfo = sayMehodInfo;
                }
                else
                {
                    askOrSayMethodInfo = interlocutor
                        .GetType()
                        .GetMethod("Ask", new[] { typeof(int), typeof(object[]) })
                        .MakeGenericMethod(returnType);

                }
                EmitHelper.GenerateSayOrAskMethodBody(
                    cordId: method.Key,
                    interlocutorSayOrAskMethodInfo: askOrSayMethodInfo,
                    interlocutorFieldInfo: outputApiFieldInfo,
                    methodBuilder: methodBuilder,
                    callParameters: method.Value.GetParameters().Select(p=>p.ParameterType).ToArray());
            }

            #endregion

            var constructorCodeGeneration = new List<Action<ILGenerator>>();

            #region interface delegate properties implementation

            foreach (var property in contractMemebers.GetProperties())
            {
                var propertyBuilder = EmitHelper.ImplementInterfaceProperty(typeBuilder, property.Value);

                var delegateInfo = ReflectionHelper.GetDelegateInfoOrNull(propertyBuilder.PropertyBuilder.PropertyType);
                if (delegateInfo == null)
                    //the property is not an delegate
                    throw new InvalidContractMemeberException(property.Value, interfaceType);

                // Create handler for every delegate property
                var handleMethodNuilder = ImplementAndGenerateHandleMethod(
                    typeBuilder, 
                    delegateInfo,
                    propertyBuilder.FieldBuilder);

                constructorCodeGeneration.Add(
                    iLGenerator => GenerateEventSubscribtion(iLGenerator, outputApiFieldInfo, property.Key, handleMethodNuilder));
            }
            #endregion

            EmitHelper.ImplementPublicConstructor(
                typeBuilder, 
                new [] {outputApiFieldInfo},
                constructorCodeGeneration);

            var finalType = typeBuilder.CreateType();
            return (T) Activator.CreateInstance(finalType, interlocutor);

        }

        public static ContractInfo ParseContractInterface(Type contractInterfaceType)
        {
            var memberInfos = new ContractInfo(contractInterfaceType);

            foreach (var methodInfo in contractInterfaceType.GetMethods())
            {
                if (methodInfo.IsSpecialName) continue;

                var attribute = Attribute.GetCustomAttribute(methodInfo,
                    typeof(ContractMessageAttribute)) as ContractMessageAttribute;
                if (attribute == null)
                    throw new ContractMemberAttributeMissingException(contractInterfaceType, methodInfo.Name);

                memberInfos.ThrowIfAlreadyContainsId(attribute.Id, methodInfo);
                memberInfos.AddInfo(attribute.Id, methodInfo);

            }
            foreach (var propertyInfo in contractInterfaceType.GetProperties())
            {
                var attribute = Attribute.GetCustomAttribute(
                    propertyInfo,
                    typeof(ContractMessageAttribute)) as ContractMessageAttribute;

                if (attribute == null)
                    throw new ContractMemberAttributeMissingException(contractInterfaceType, propertyInfo.Name);

                memberInfos.ThrowIfAlreadyContainsId(attribute.Id, propertyInfo);
                memberInfos.AddInfo(attribute.Id, propertyInfo);
            }
            return memberInfos;
        }

        private static TypeBuilder CreateProxyTypeBuilder<T>()
        {
            var typeCount = Interlocked.Increment(ref _exemmplarCounter);
            return EmitHelper.CreateTypeBuilder(typeof(T).Name + "_" + typeCount);
        }

        

        private static MethodBuilder ImplementAndGenerateHandleMethod(
            TypeBuilder typeBuilder,
            DelegatePropertyInfo delegatePropertyInfo,
            FieldInfo delegateFieldInfo)
        {
            /* ******************For Say Calls:******************
             * 
             * Action<int,DateTime,object, string> _originDelegatePropertyField;
             * 
             * public Action<int,DateTime,object, string,string> originDelegateProperty{ 
             *       get{return _originDelegatePropertyField;}
             *       set{_originDelegatePropertyField = value;}
             * }
             * 
             * private string Handle_originDelegatePropertyField(object[] arguments)
             * {
             *   ///we generate this code:
             *   var originDelegate = _originDelegatePropertyField;
		     *   if(originDelegate == null)
			 *      return 
		     *
			 *   originDelegate((int)  arguments[0], 
			 *	             (DateTime) arguments[1], 
			 *	             (object)   arguments[2],
			 *		         (string)   arguments[3]);
             *  }
             *
             * ******************For Ask Calls:******************
             * 
             * Func<int,DateTime,object, string,string> _originDelegatePropertyField;
             * 
             * public Func<int,DateTime,object, string,string> originDelegateProperty{ 
             *       get{return _originDelegatePropertyField;}
             *       set{_originDelegatePropertyField = value;}
             * }
             * 
             * private string Handle_originDelegatePropertyField(object[] arguments)
             * {
             *   ///we generate this code:
             *   var originDelegate = _originDelegatePropertyField;
		     *   
             *   string returnValue = null; ///а должно быть default(string)
             *   
             *   if(originDelegate!=null)
			 *     returnValue =  originDelegate((int)  arguments[0], 
			 *	             (DateTime) arguments[1], 
			 *	             (object)   arguments[2],
			 *		         (string)   arguments[3]);
             *
             *     return returnValue;
             *  }
             *  
             *  
             *  
             *  
             */

            var id = Interlocked.Increment(ref _exemmplarCounter);

            //build Handle method:
            var handleMethodBuilder = typeBuilder.DefineMethod(
                name: "Handle" + delegateFieldInfo.Name + id,
                attributes: MethodAttributes.Public,
                returnType: delegatePropertyInfo.ReturnType,
                parameterTypes: new[] {typeof(object[])});

            //
            ILGenerator ilGen = handleMethodBuilder.GetILGenerator();
            var hasReturnType = delegatePropertyInfo.ReturnType != typeof(void);
            LocalBuilder returnValue = null;

            if (hasReturnType)
            {
                //create local variable returnValue, equals zero
                returnValue = ilGen.DeclareLocal(delegatePropertyInfo.ReturnType);
                //we need set default(delegatePropertyInfo.ReturnType), but somehow it works. Little bit strange...
                ilGen.Emit(OpCodes.Ldnull);
                ilGen.Emit(OpCodes.Stloc, returnValue);
            }

            ilGen.Emit(OpCodes.Ldarg_0);

            //check weather delegate == null
            ilGen.Emit(OpCodes.Ldfld, delegateFieldInfo);
            var delegateFieldValue = ilGen.DeclareLocal(delegateFieldInfo.FieldType);

            ilGen.Emit(OpCodes.Stloc, delegateFieldValue);
            ilGen.Emit(OpCodes.Ldloc, delegateFieldValue);

            ilGen.Emit(OpCodes.Ldnull);
            ilGen.Emit(OpCodes.Ceq);

            var finishLabel = ilGen.DefineLabel();
            //if field == null  than return
            ilGen.Emit(OpCodes.Brtrue_S, finishLabel);

            ilGen.Emit(OpCodes.Ldloc, delegateFieldValue);

            int i = 0;
            //наполняем стек списком аргументов вызова
            foreach (var parameterType in delegatePropertyInfo.ParameterTypes)
            {
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldc_I4, i);
                ilGen.Emit(OpCodes.Ldelem_Ref);

                if (parameterType.IsValueType)
                    ilGen.Emit(OpCodes.Unbox_Any, parameterType);
                else /*if (parameterType!= typeof(object))*/
                    ilGen.Emit(OpCodes.Castclass, parameterType);
                i++;
            }

            ilGen.Emit(OpCodes.Callvirt, delegatePropertyInfo.DelegateInvokeMethodInfo);

            if (hasReturnType)
            {
                //выставляем в переменную returnValue результат вызова делегата
                ilGen.Emit(OpCodes.Stloc, returnValue);
            }
            ilGen.MarkLabel(finishLabel);

            if (hasReturnType)
                ilGen.Emit(OpCodes.Ldloc, returnValue);

            ilGen.Emit(OpCodes.Ret);

            return handleMethodBuilder;
        }

        private static void GenerateEventSubscribtion(
            ILGenerator methodIlGenerator, 
            FieldInfo rawContractFieldInfo,
            int cordId, MethodInfo handleMethod)
        {
            // Код в конструкторе
            // 
            // Ask:
            //_rawContract.Subscribe<string>(71, HandleOnTick); //HandleOnTick - метод обработки
            //
            //
            // Say:
            //_rawContract.Subscribe(71,HandleOnTick);

            //  IL_000F: ldarg.0
            //IL_0010: ldfld UserQuery+SayingContract._rawContract
            //IL_0015: ldc.i4.s    47
            //IL_0017: ldarg.0
            //IL_0018: ldftn UserQuery+SayingContract.HandleOnTick
            //IL_001E: newobj System.Func<System.Object[], System.String>..ctor
            //IL_0023: callvirt UserQuery+IRawOutputContract.Subscribe<String>

            var il = methodIlGenerator;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, rawContractFieldInfo);
            il.Emit(OpCodes.Ldc_I4, cordId);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldftn, handleMethod);

            if (handleMethod.ReturnType == typeof(void))
            {
                var actionType = typeof(Action<>).MakeGenericType(typeof(object[]));
                var actionConstructor = actionType.GetConstructor(new[] {typeof(object), typeof(IntPtr)});
                il.Emit(OpCodes.Newobj, actionConstructor);
                var subscribeMethod = typeof(ICordInterlocutor).GetMethod("SaySubscribe", new[] {typeof(int), actionType});
                il.Emit(OpCodes.Callvirt, subscribeMethod);
            }
            else
            {
                var funkType = typeof(Func<,>).MakeGenericType(typeof(object[]), handleMethod.ReturnType);
                var funcConstructor = funkType.GetConstructor(new[] {typeof(object), typeof(IntPtr)});

                il.Emit(OpCodes.Newobj, funcConstructor);
                var subscribeMethodInfo = typeof(ICordInterlocutor).GetMethod("AskSubscribe");

                var subscribeMethodGenericInfo = subscribeMethodInfo.MakeGenericMethod(handleMethod.ReturnType);
                il.Emit(OpCodes.Callvirt, subscribeMethodGenericInfo);
            }
        }
    }
}
