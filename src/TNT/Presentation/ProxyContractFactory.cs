using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using TNT.Exceptions;

namespace TNT.Presentation
{
    public static class ProxyContractFactory
    {
        private static int _exemmplarCounter;

        public static T CreateProxyContract<T>(ICordInterlocutor rawOutput)
        {
            var interfaceType = typeof(T);
            TypeBuilder typeBuilder = CreateProxyTypeBuilder<T>();

            typeBuilder.AddInterfaceImplementation(interfaceType);

            const string outputApiFieldName = "_outputApi";
            var outputApiFieldInfo = typeBuilder.DefineField(
                                        outputApiFieldName, 
                                        typeof(ICordInterlocutor),
                                        FieldAttributes.Private);
            
            var sayMehodInfo = rawOutput.GetType().GetMethod("Say", new[] {typeof(int), typeof(object[])});



            Dictionary<int, MemberInfo> cordIdUsedNames = new Dictionary<int, MemberInfo>();


            foreach (var eventInfo in interfaceType.GetEvents())
            {
                throw new InvalidContractMemeberException(eventInfo, interfaceType);
            }


            #region реализуем методы интерфейса

            foreach (var methodInfo in interfaceType.GetMethods())
            {
                if (methodInfo.IsSpecialName) continue;

                var attribute = Attribute.GetCustomAttribute(methodInfo,
                    typeof(ContractMessageAttribute)) as ContractMessageAttribute;
                if (attribute == null)
                    throw new ContractMemberAttributeMissingException(interfaceType,methodInfo.Name);

                ThrowIfDuplicateAndRememberOtherwise(interfaceType, cordIdUsedNames, methodInfo, attribute.Id);

                var methodBuilder = EmitHelper.ImplementInterfaceMethod(methodInfo, typeBuilder);

                var returnType = methodInfo.ReturnType;
                MethodInfo askOrSayMethodInfo = null;
                if (returnType == typeof(void))
                {
                    askOrSayMethodInfo = sayMehodInfo;
                }
                else
                {
                    askOrSayMethodInfo = rawOutput
                        .GetType()
                        .GetMethod("Ask", new[] { typeof(int), typeof(object[]) })
                        .MakeGenericMethod(returnType);

                }
                GenerateSayOrAskMethodBody(
                    id: attribute.Id,
                    outputApiSayOrAskMethodInfo: askOrSayMethodInfo,
                    outputApiFieldInfo: outputApiFieldInfo,
                    interfaceMethodInfo: methodInfo,
                    proxyMethodBuilder: methodBuilder);
            }

            #endregion

            List<Action<ILGenerator>> constructorCodeGeneration = new List<Action<ILGenerator>>();

            #region реализуем делегат свойства интерфейса

            foreach (var propertyInfo in interfaceType.GetProperties())
            {
                var attribute = Attribute.GetCustomAttribute(
                    propertyInfo, 
                    typeof(ContractMessageAttribute)) as ContractMessageAttribute;

                if (attribute == null)
                    throw new ContractMemberAttributeMissingException(interfaceType, propertyInfo.Name);

                ThrowIfDuplicateAndRememberOtherwise(interfaceType, cordIdUsedNames, propertyInfo, attribute.Id);

                var propertyBuilder = EmitHelper.ImplementInterfaceProperty(typeBuilder, propertyInfo);

                var delegateInfo = EmitHelper.GetDelegateInfoOrNull(propertyBuilder.PropertyBuilder.PropertyType);
                if (delegateInfo == null)
                    //свойство не является делегатом
                    throw new InvalidContractMemeberException(propertyInfo,interfaceType);

                // теперь для каждого делегат свойства нужно сделать хендлер
                var handleMethodNuilder = ImplementAndGenerateHandleMethod(
                    typeBuilder, 
                    delegateInfo,
                    propertyBuilder.FieldBuilder);

                constructorCodeGeneration.Add(
                    iLGenerator => GenerateEventSubscribtion(iLGenerator, outputApiFieldInfo, attribute.Id, handleMethodNuilder));
            }
            #endregion

            EmitHelper.ImplementPublicConstructor(
                typeBuilder, 
                new [] {outputApiFieldInfo},
                constructorCodeGeneration);

            var finalType = typeBuilder.CreateType();
            return (T) Activator.CreateInstance(finalType, rawOutput);

        }

        private static void ThrowIfDuplicateAndRememberOtherwise(Type interfaceType, 
            Dictionary<int, MemberInfo> cordIdUsedNames, 
            MemberInfo methodInfo, 
            int cordId)
        {
            if (cordIdUsedNames.ContainsKey(cordId))
                throw new ContractCordIdDuplicateException(cordId, interfaceType, cordIdUsedNames[cordId].Name, methodInfo.Name);
            cordIdUsedNames.Add(cordId, methodInfo);
        }

        private static TypeBuilder CreateProxyTypeBuilder<T>()
        {
            var dynGeneratorHostAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("Test.Gen, Version=1.0.0.1"),
                AssemblyBuilderAccess.Run);
            var dynModule = dynGeneratorHostAssembly.DefineDynamicModule(
                "Test.Gen.Mod");
            var typeCount = Interlocked.Increment(ref _exemmplarCounter);

            TypeBuilder tb = dynModule.DefineType(typeof(T).Name + "_" + typeCount, TypeAttributes.Public);
            return tb;
        }

        private static void GenerateSayOrAskMethodBody(
            int           id,
            FieldInfo     outputApiFieldInfo,
            MethodInfo    outputApiSayOrAskMethodInfo,
            MethodInfo    interfaceMethodInfo,
            MethodBuilder proxyMethodBuilder
        )
        {
            var callParameters = interfaceMethodInfo.GetParameters();
            /*
             * For non return (SAY) methods: 
             * 
             *  public void ContractMessage(int intParameter, /.../ double doubleParameter)
             *  {
             *      _rawContract.Say({CordId} , new object []{ intParameter,/.../ doubleParameter });
             *  }
             *  
             *  
             *  For methods with return (ASK):
             *  
             *  public int ContractMessage(int intParameter, /.../ double doubleParameter)
             *  {
             *     return _rawContract.Ask<int>({CordId} , new object []{ intParameter,/.../ doubleParameter });
             *  }
             *  
             */
             
            ILGenerator ilGen = proxyMethodBuilder.GetILGenerator();
            // ставим на стек сам прокси объект 
            ilGen.Emit(OpCodes.Ldarg_0);
            // ставим на стек ссылку на поле _outputApi
            ilGen.Emit(OpCodes.Ldfld, outputApiFieldInfo);
            // готовимся к вызову _outputApi.Say(id, object[])
            // ставим на стек id:
            ilGen.Emit(OpCodes.Ldc_I4, id);

            // ставим на стек размер массива:
            ilGen.Emit(OpCodes.Ldc_I4, callParameters.Length);
            // создаём массив на стеке
            ilGen.Emit(OpCodes.Newarr, typeof(object));
            // заполняем массив:
            for (int j = 0; j < callParameters.Length; j++)
            {
                ilGen.Emit(OpCodes.Dup); // т.к. Stelem_Ref удалит ссылку на массив - нужно её продублировать

                ilGen.Emit(OpCodes.Ldc_I4, j); //ставим индекс массива
                ilGen.Emit(OpCodes.Ldarg, j + 1); //грузим аргумент вызова
                if (callParameters[j].ParameterType.IsValueType) //если это Value Type то боксим
                    ilGen.Emit(OpCodes.Box, callParameters[j].ParameterType);
                // значения стека сейчас:
                // 0 - значение
                // 1 - индекс массива
                // 2 - массив
                ilGen.Emit(OpCodes.Stelem_Ref); //грузим в массив 
            }

            // вызываем Say или Ask:
            ilGen.Emit(OpCodes.Callvirt, outputApiSayOrAskMethodInfo);
            // Если это был Ask метод то он положин на верхушку стека свой результат
            ilGen.Emit(OpCodes.Ret);
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

            //Строим Handle - метод:
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
                //cоздаём локальную переменную returnValue, равную нулю
                returnValue = ilGen.DeclareLocal(delegatePropertyInfo.ReturnType);
                //Сюда должен ставится default(delegatePropertyInfo.ReturnType)
                //Хотя почему то работает и так. Странно
                ilGen.Emit(OpCodes.Ldnull);
                ilGen.Emit(OpCodes.Stloc, returnValue);
            }

            ilGen.Emit(OpCodes.Ldarg_0);

            //проверяем что delegate == null
            ilGen.Emit(OpCodes.Ldfld, delegateFieldInfo);
            var delegateFieldValue = ilGen.DeclareLocal(delegateFieldInfo.FieldType);

            ilGen.Emit(OpCodes.Stloc, delegateFieldValue);
            ilGen.Emit(OpCodes.Ldloc, delegateFieldValue);

            ilGen.Emit(OpCodes.Ldnull);
            ilGen.Emit(OpCodes.Ceq);

            var finishLabel = ilGen.DefineLabel();
            //если поле == null то сразу выходим
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
