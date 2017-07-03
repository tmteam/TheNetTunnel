using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmitExperiments
{
    public static class ProxyContractFactory
    {
        private static int _exemmplarCounter = 0;

        public static T CreateProxyContract<T>(IOutputCordApi rawOutput)
        {
            var interfaceType = typeof(T);
            TypeBuilder typeBuilder = CreateProxyTypeBuilder<T>();

            typeBuilder.AddInterfaceImplementation(interfaceType);

            const string outputApiFieldName = "_outputApi";
            var outputApiFieldInfo = typeBuilder.DefineField(outputApiFieldName, typeof(IOutputCordApi),
                FieldAttributes.Private);


            var sayMehodInfo = rawOutput.GetType().GetMethod("Say", new[] {typeof(int), typeof(object[])});


            #region реализуем методы интерфейса

            foreach (var methodInfo in interfaceType.GetMethods())
            {
                if (methodInfo.IsSpecialName)
                    continue;
                var attribute =
                    Attribute.GetCustomAttribute(methodInfo, typeof(ContractMessageAttribute)) as
                        ContractMessageAttribute;
                if (attribute == null)
                    throw new InvalidOperationException("no ContractMessageAttribute");

                var methodBuilder = EmitHelper.ImplementInterfaceMethod(methodInfo, typeBuilder);

                var returnType = methodInfo.ReturnType;
                MethodInfo askOrSayMethodInfo = null;
                if (returnType == typeof(void))
                {
                    askOrSayMethodInfo = sayMehodInfo;
                }
                else
                {
                    askOrSayMethodInfo
                        = rawOutput.GetType().GetMethod("Ask", new[] {typeof(int), typeof(object[])})
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
                var attribute =
                    Attribute.GetCustomAttribute(propertyInfo, typeof(ContractMessageAttribute)) as
                        ContractMessageAttribute;
                if (attribute == null)
                    throw new InvalidOperationException("no ContractMessageAttribute");

                var propertyBuilder = EmitHelper.ImplementInterfaceProperty(typeBuilder, propertyInfo);

                var delegateInfo = EmitHelper.GetDelegateInfoOrNull(propertyBuilder.PropertyBuilder.PropertyType);
                if (delegateInfo == null)
                    //свойство не является делегатом
                    throw new InvalidOperationException("Property " + propertyBuilder.PropertyBuilder.Name + " is not a delegate");

                // теперь для каждого делегат свойства нужно сделать хендлер
                var handleMethodNuilder = ImplementAndGenerateHandleMethod(typeBuilder, delegateInfo,
                    propertyBuilder.FieldBuilder);

                constructorCodeGeneration.Add(
                    (iLGenerator) =>
                        GenerateEventSubscribtion(iLGenerator, outputApiFieldInfo, attribute.Id, handleMethodNuilder));
            }

            #endregion

            EmitHelper.ImplementPublicConstructor(typeBuilder, new FieldBuilder[] {outputApiFieldInfo}, constructorCodeGeneration);

            var finalType = typeBuilder.CreateType();
            return (T)Activator.CreateInstance(finalType, rawOutput);

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
            int id, 
            FieldInfo outputApiFieldInfo, 
            MethodInfo outputApiSayOrAskMethodInfo,
            MethodInfo interfaceMethodInfo, 
            MethodBuilder proxyMethodBuilder
            )
        {

            var callParameters = interfaceMethodInfo.GetParameters();

            /*
             *  public void ContractMessage(int intParameter, /.../ double doubleParameter)
             *  {
             *      _rawContract.Say({CordId} , new object []{ intParameter,/.../ doubleParameter });
             *  }
             */


            ILGenerator ilGen = proxyMethodBuilder.GetILGenerator();
            //ставим на стек сам прокси обыект 
            ilGen.Emit(OpCodes.Ldarg_0);
            //ставим на стек ссылку на поле _outputApi
            ilGen.Emit(OpCodes.Ldfld, outputApiFieldInfo);

            //готовимся к вызову _outputApi.Say(id, object[])
            // ставим на стек id:
            ilGen.Emit(OpCodes.Ldc_I4, id);

            // ставим на стек размер массива:
            ilGen.Emit(OpCodes.Ldc_I4, callParameters.Length);
            ////создаём массив на стеке
            ilGen.Emit(OpCodes.Newarr, typeof(object));
            //заполняем массив:
            for (int j = 0; j < callParameters.Length; j++)
            {
                ilGen.Emit(OpCodes.Dup); // т.к. Stelem_Ref удалит ссылку на массив - нужно её продублировать

                ilGen.Emit(OpCodes.Ldc_I4, j);//ставим индекс массива
                ilGen.Emit(OpCodes.Ldarg, j + 1);//грузим аргумент вызова
                if (callParameters[j].ParameterType.IsValueType)//если это вэлу тайп то боксим
                    ilGen.Emit(OpCodes.Box, callParameters[j].ParameterType);
                //значения стека сейчас:
                // 0 - значение
                // 1 - индекс массива
                // 2 - массив
                ilGen.Emit(OpCodes.Stelem_Ref);//грузим в массив 
            }


            ////вызываем Say или Ask:
            ilGen.Emit(OpCodes.Callvirt, outputApiSayOrAskMethodInfo);
            //Если это был Ask метод то он положин на верхушку стека свой результат
            ilGen.Emit(OpCodes.Ret);
        }


       

        static MethodBuilder ImplementAndGenerateHandleMethod(
            TypeBuilder typeBuilder, 
            DelegatePropertyInfo delegatePropertyInfo,
            FieldInfo delegateFieldInfo
            

            )
        {
            /*
             * private string HandleOnMessage(object[] arguments)
             * {
             * var originDelegate = originDelegateProperty;
		        if(originDelegate == null)
			        return default(string);
		
			        return originDelegate((int)  arguments[0], 
					        (DateTime) arguments[1], 
					        (object)   arguments[2],
					        (string)   arguments[3]);
                }
             */

            var id = Interlocked.Increment(ref _exemmplarCounter);

            var handleMethodBuilder = typeBuilder.DefineMethod(
                name: "Handle" + delegateFieldInfo.Name + id, 
                attributes: MethodAttributes.Public,
                returnType: delegatePropertyInfo.ReturnType,
                parameterTypes: new [] {typeof(object[])});


            ILGenerator ilGen = handleMethodBuilder.GetILGenerator();
            //var hasReturnType = delegatePropertyInfo.ReturnType != typeof(void);
            ////Ставим в  переменную null
            //if (hasReturnType)
            //{
            //    ilGen.Emit(OpCodes.Ldnull);
            //    ilGen.Emit(OpCodes.Stloc_0);
            //}
            
            ilGen.Emit(OpCodes.Ldarg_0);

            //check delegate == null
            ilGen.Emit(OpCodes.Ldfld, delegateFieldInfo);
            var delegateFieldValue = ilGen.DeclareLocal(delegateFieldInfo.FieldType);
                  
            ilGen.Emit(OpCodes.Stloc, delegateFieldValue);
            ilGen.Emit(OpCodes.Ldloc, delegateFieldValue);
            
            ilGen.Emit(OpCodes.Ldnull);
            ilGen.Emit(OpCodes.Ceq);

            var isDelegateNull = ilGen.DeclareLocal(typeof(int));

            ilGen.Emit(OpCodes.Stloc, isDelegateNull);
            ilGen.Emit(OpCodes.Ldloc, isDelegateNull);
           // ilGen.EmitWriteLine("IsDelegateNull:");
           // ilGen.EmitWriteLine(isDelegateNull);

            //ilGen.Emit(OpCodes.Pop);
            
            var finishLabel = ilGen.DefineLabel();
            //если поле == null то сразу выходим
            ilGen.Emit(OpCodes.Brtrue_S, finishLabel);

           // ilGen.EmitWriteLine("Delegate is not null");

            ilGen.Emit(OpCodes.Ldloc, delegateFieldValue);

            int i = 0;
            //иначе 
            foreach (var parameterType in delegatePropertyInfo.ParameterTypes)
            {
                ilGen.Emit(OpCodes.Ldarg_1);

                ilGen.Emit(OpCodes.Ldc_I4, i);


                ilGen.Emit(OpCodes.Ldelem_Ref);

                var arrayValue = ilGen.DeclareLocal(typeof(object));

                ilGen.Emit(OpCodes.Stloc, arrayValue);
                 ilGen.Emit(OpCodes.Ldloc, arrayValue);
                //ilGen.EmitWriteLine(arrayValue);


                if (parameterType.IsValueType)
                    ilGen.Emit(OpCodes.Unbox_Any, parameterType);
                else /*if (parameterType!= typeof(object))*/
                    ilGen.Emit(OpCodes.Castclass, parameterType);
                i++;
            }
            

         
            ilGen.Emit(OpCodes.Callvirt, delegatePropertyInfo.DelegateInvokeMethodInfo);

            //if (hasReturnType)
            //    ilGen.Emit(OpCodes.Stloc_1);



            ilGen.MarkLabel(finishLabel);
             
           //  if (hasReturnType)
           //      ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Ret);

            return handleMethodBuilder;
        }

        private static void GenerateEventSubscribtion(ILGenerator methodIlGenerator, FieldInfo rawContractFieldInfo, int cordId, MethodInfo handleMethod )
        {
            //_rawContract.Subscribe<string>(71, HandleOnTick); //HandleOnTick - метод обработки
            //or
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
                var actionConstructor = actionType.GetConstructor(new [] { typeof(object), typeof(IntPtr)});
                il.Emit(OpCodes.Newobj, actionConstructor);
                var subscribeMethod =
                    typeof(IOutputCordApi).GetMethod("SaySubscribe", new[] {typeof(int), actionType });
                il.Emit(OpCodes.Callvirt, subscribeMethod);
            }
            else
            {
                var funkType = typeof(Func<,>).MakeGenericType(typeof(object[]), handleMethod.ReturnType); 
                var funcConstructor = funkType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });

                il.Emit(OpCodes.Newobj, funcConstructor);
                var subscribeMethodInfo = typeof(IOutputCordApi).GetMethod("AskSubscribe");

                var subscribeMethodGenericInfo  = subscribeMethodInfo.MakeGenericMethod(handleMethod.ReturnType);
                il.Emit(OpCodes.Callvirt, subscribeMethodGenericInfo);
            }
        }
    }
}
