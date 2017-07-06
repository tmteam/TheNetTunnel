using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace EmitExperiments
{
    public static class EmitHelper
    {
        public static void ImplementPublicConstructor(
            TypeBuilder tb, 
            FieldBuilder[] filedsNeedToBeSetted,
            IEnumerable<Action<ILGenerator>> additionalCodeGenerators)
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
            foreach (var additionalCodeGenerator in additionalCodeGenerators)
            {
                additionalCodeGenerator(il);
            }
            il.Emit(OpCodes.Ret);
        }

        public static DelegatePropertyInfo GetDelegateInfoOrNull(Type delegateType)
        {
            var ainvk = delegateType.GetMethod("Invoke");
            if (ainvk == null)
                return null;
            var parameters = ainvk.GetParameters().Select(p => p.ParameterType).ToArray();
            var returnType = ainvk.ReturnParameter.ParameterType;
            return new DelegatePropertyInfo
            {
                DelegateInvokeMethodInfo = ainvk,
                ParameterTypes = parameters,
                ReturnType = returnType,
            };

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

        public class ImplementInterfacePropertyResults
        {
            public PropertyBuilder PropertyBuilder { get; set; }
            public FieldBuilder FieldBuilder { get; set; }
        }
    }
}
