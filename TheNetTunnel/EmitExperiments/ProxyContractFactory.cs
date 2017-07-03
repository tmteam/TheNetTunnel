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
            var outputApiFieldInfo = typeBuilder.DefineField(outputApiFieldName, typeof(IOutputCordApi), FieldAttributes.Private);

            CreateProxyConstructor(typeBuilder, outputApiFieldInfo);

            var sayMehodInfo = rawOutput.GetType().GetMethod("Say", new[] {typeof(int), typeof(object[]) });


            #region реализуем методы интерфейса
            foreach (var methodInfo in interfaceType.GetMethods())
            {
                if(methodInfo.IsSpecialName)
                    continue;
                var attribute = Attribute.GetCustomAttribute(methodInfo, typeof(ContractMessageAttribute)) as ContractMessageAttribute;
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
                        = rawOutput.GetType().GetMethod("Ask", new[] { typeof(int), typeof(object[]) })
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

            #region реализуем делегат свойства интерфейса

            foreach (var propertyInfo in interfaceType.GetProperties())
            {
                var attribute = Attribute.GetCustomAttribute(propertyInfo, typeof(ContractMessageAttribute)) as ContractMessageAttribute;
                if (attribute == null)
                    throw new InvalidOperationException("no ContractMessageAttribute");

                var propertyBuilder = EmitHelper.ImplementInterfaceProperty(typeBuilder, propertyInfo);
            }

            #endregion

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

        private static void CreateProxyConstructor(TypeBuilder tb, FieldBuilder outputApiFieldInfo)
        {
            var constructorInfo = tb.DefineConstructor(
                MethodAttributes.Public, 
                CallingConventions.Standard, 
                new[] { typeof(IOutputCordApi) });
            var il = constructorInfo.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, outputApiFieldInfo);
            il.Emit(OpCodes.Ret);
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
            DelegatePropertyInfo delegatePropertyInfo
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
                name: "Handle" + delegatePropertyInfo.HandleDelegateGetMethodInfo.Name + id, 
                attributes: MethodAttributes.Private,
                returnType: delegatePropertyInfo.ReturnType,
                parameterTypes: new [] {typeof(object[])});


            ILGenerator ilGen = handleMethodBuilder.GetILGenerator();
            var hasReturnType = delegatePropertyInfo.ReturnType != typeof(void);
            //Ставим в  переменную null
            if (hasReturnType)
            {
                ilGen.Emit(OpCodes.Ldnull);
                ilGen.Emit(OpCodes.Stloc_1);
            }
            ilGen.Emit(OpCodes.Ldarg_0);
            //check delegate == null
            ilGen.Emit(OpCodes.Call, delegatePropertyInfo.HandleDelegateGetMethodInfo);
            ilGen.Emit(OpCodes.Dup);
            ilGen.Emit(OpCodes.Ldnull);
            ilGen.Emit(OpCodes.Ceq);

            var finishLabel = new Label();
            //если поле == null то сразу выходим
            ilGen.Emit(OpCodes.Brtrue_S, finishLabel);

            int i = 0;
            //иначе 
            foreach (var parameterType in delegatePropertyInfo.ParameterTypes)
            {
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldc_I4, i);
                ilGen.Emit(OpCodes.Ldelem_Ref);
                if (parameterType.IsValueType)
                    ilGen.Emit(OpCodes.Unbox_Any, parameterType);
                else if (parameterType!= typeof(object))
                    ilGen.Emit(OpCodes.Castclass, parameterType);
                i++;
            }

            ilGen.EmitCall(OpCodes.Callvirt, delegatePropertyInfo.DelegateInvokeMethodInfo, null);
            if (hasReturnType)
                ilGen.Emit(OpCodes.Stloc_1);

            ilGen.MarkLabel(finishLabel);

            if (hasReturnType)
                ilGen.Emit(OpCodes.Ldloc_1);
            ilGen.Emit(OpCodes.Ret);

            return handleMethodBuilder;
        }
    }
}
