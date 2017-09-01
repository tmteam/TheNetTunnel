using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TNT.Contract
{
    public static class EmitHelper
    {
        public static void ImplementPublicConstructor(
            TypeBuilder tb, 
            FieldBuilder[] filedsNeedToBeSetted,
            IEnumerable<Action<ILGenerator>> additionalCodeGenerators = null)
        {
            var constructorInfo = tb.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                filedsNeedToBeSetted.Select(f => f.FieldType).ToArray());

            var il = constructorInfo.GetILGenerator();
            int fieldNumber = 1;

            /*
             *  public MyType(type1 field1, type2 field2 ...)
             *  {
             *   _field1 = field1;
             *   _field2 = field2;
             *   ...
             *   
             *   /// additionalCode #1
             *   
             *   ///additionalCode #1
             *   ...
             *  }
             */

            foreach (var fieldBuilder in filedsNeedToBeSetted)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg, fieldNumber);
                il.Emit(OpCodes.Stfld, fieldBuilder);
                fieldNumber++;
            }
            if (additionalCodeGenerators != null)
            {
                foreach (var additionalCodeGenerator in additionalCodeGenerators)
                {
                    additionalCodeGenerator(il);
                }
            }
            il.Emit(OpCodes.Ret);
        }

        public static TypeBuilder CreateTypeBuilder(string typeName, string assemblyName = "AutogenAssembly", string moduleName = "AutogenAssembly.Module")
        {
            var dynGeneratorHostAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(assemblyName+", Version=1.0.0.1"),
                AssemblyBuilderAccess.Run);
            var dynModule = dynGeneratorHostAssembly.DefineDynamicModule(moduleName);

            TypeBuilder tb = dynModule.DefineType(typeName, TypeAttributes.Public);
            return tb;
        }

        public static MethodBuilder ImplementInterfaceMethod(MethodInfo interfaceMethodInfo, TypeBuilder typeBuilder)
        {
            Type[] inputParams = interfaceMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            Type outputParams = interfaceMethodInfo.ReturnType;


            MethodBuilder metbuilder;
            if (!inputParams.Any() && outputParams == typeof(void))
                metbuilder = typeBuilder.DefineMethod(interfaceMethodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual);
            else
                metbuilder = typeBuilder.DefineMethod(interfaceMethodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual, outputParams, inputParams);

            typeBuilder.DefineMethodOverride(metbuilder, interfaceMethodInfo);
            return metbuilder;
        }


        public static ImplementInterfacePropertyResults ImplementInterfaceProperty(
            TypeBuilder typeBuilder,
            PropertyInfo interfacePropertyInfo)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + interfacePropertyInfo.Name,
                interfacePropertyInfo.PropertyType,
                FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(
                interfacePropertyInfo.Name,
                PropertyAttributes.HasDefault,
                interfacePropertyInfo.PropertyType,
                null);

            #region создаём GetMethod

            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + interfacePropertyInfo.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                MethodAttributes.Virtual,
                interfacePropertyInfo.PropertyType, Type.EmptyTypes);


            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            #endregion

            #region создаём SetMethod

            MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + interfacePropertyInfo.Name,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig |
                MethodAttributes.Virtual,
                null, new[] {interfacePropertyInfo.PropertyType});

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();

            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);

            setIl.Emit(OpCodes.Ret);

            #endregion

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
            typeBuilder.DefineMethodOverride(getPropMthdBldr, interfacePropertyInfo.GetMethod);
            typeBuilder.DefineMethodOverride(setPropMthdBldr, interfacePropertyInfo.SetMethod);

            return new ImplementInterfacePropertyResults
            {
                FieldBuilder = fieldBuilder,
                PropertyBuilder = propertyBuilder
            };
        }


        public static void GenerateSayOrAskMethodBody(
            int messageTypeId,
            FieldInfo interlocutorFieldInfo,
            MethodInfo interlocutorSayOrAskMethodInfo,
            MethodBuilder methodBuilder,
            Type[] callParameters
        )
        {
            /*
             * For non return (SAY) methods: 
             * 
             *  public void ContractMessage(int intParameter, /.../ double doubleParameter)
             *  {
             *      _interlocutor.Say({messageTypeId} , new object []{ intParameter,/.../ doubleParameter });
             *  }
             *  
             *  
             *  For methods with return (ASK):
             *  
             *  public int ContractMessage(int intParameter, /.../ double doubleParameter)
             *  {
             *     return _interlocutor.Ask<int>({messageTypeId} , new object []{ intParameter,/.../ doubleParameter });
             *  }
             *  
             */

            ILGenerator ilGen = methodBuilder.GetILGenerator();
            // ставим на стек сам прокси объект 
            ilGen.Emit(OpCodes.Ldarg_0);
            // ставим на стек ссылку на поле _outputApi
            ilGen.Emit(OpCodes.Ldfld, interlocutorFieldInfo);
            // готовимся к вызову _interlocutor.Say(id, object[])
            // ставим на стек id:
            ilGen.Emit(OpCodes.Ldc_I4, messageTypeId);

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
                if (callParameters[j].GetTypeInfo().IsValueType) //если это Value Type то боксим
                    ilGen.Emit(OpCodes.Box, callParameters[j]);
                // значения стека сейчас:
                // 0 - значение
                // 1 - индекс массива
                // 2 - массив
                ilGen.Emit(OpCodes.Stelem_Ref); //грузим в массив 
            }

            // вызываем Say или Ask:
            ilGen.Emit(OpCodes.Callvirt, interlocutorSayOrAskMethodInfo);
            // Если это был Ask метод то он положин на верхушку стека свой результат
            ilGen.Emit(OpCodes.Ret);
        }


        public class ImplementInterfacePropertyResults
        {
            public PropertyBuilder PropertyBuilder { get; set; }
            public FieldBuilder FieldBuilder { get; set; }
        }
    }
}
