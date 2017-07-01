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

            foreach (var methodInfo in interfaceType.GetMethods())
            {
                if(methodInfo.IsSpecialName)
                    continue;
                var attribute = Attribute.GetCustomAttribute(methodInfo, typeof(ContractMessageAttribute)) as ContractMessageAttribute;
                if (attribute == null)
                    throw new InvalidOperationException("no ContractMessageAttribute");

                var methodBuilder = ImplementMethod(methodInfo, typeBuilder);

                GenerateSayingMethod(
                    id: attribute.Id,
                    outputApiSayMethodInfo: sayMehodInfo,
                    outputApiFieldInfo: outputApiFieldInfo,
                    interfaceMethodInfo: methodInfo,
                    proxyMethodBuilder: methodBuilder);

            }
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

        private static void GenerateSayingMethod(
            int id, 
            FieldInfo outputApiFieldInfo, 
            MethodInfo outputApiSayMethodInfo,
            MethodInfo interfaceMethodInfo, 
            MethodBuilder proxyMethodBuilder
            )
        {

            var callParameters = interfaceMethodInfo.GetParameters();


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


            ////вызываем Say:
            ilGen.Emit(OpCodes.Callvirt, outputApiSayMethodInfo);
            ilGen.Emit(OpCodes.Ret);
        }

        static MethodBuilder ImplementMethod(MethodInfo interfaceMethodInfo, TypeBuilder typeBuilder)
        {
            Type[] inputParams = interfaceMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            Type outputParams = interfaceMethodInfo.ReturnType;


            MethodBuilder metbuilder;
            if (!inputParams.Any() && outputParams == typeof(void))
                metbuilder = typeBuilder.DefineMethod(interfaceMethodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual);
            else
                metbuilder = typeBuilder.DefineMethod(interfaceMethodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, outputParams, inputParams);

            typeBuilder.DefineMethodOverride(metbuilder, interfaceMethodInfo);
            return metbuilder;
        }
    }
}
