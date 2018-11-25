using System;
using System.Linq;
using System.Reflection;

namespace TNT.Contract
{
    public static class ReflectionHelper
    {
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

        public static PropertyInfo GetOverridedPropertyOrNull(Type targetType, PropertyInfo baseImplementationProperty)
        {
            return targetType.GetProperty(baseImplementationProperty.Name, baseImplementationProperty.PropertyType);
        }

        public static MethodInfo GetOverridedMethodOrNull(Type targetType, MethodInfo baseImplementationMethod)
        {
            return targetType.GetMethod(baseImplementationMethod.Name,
                   baseImplementationMethod.GetParameters().Select(p => p.ParameterType).ToArray());
        }
    }
}
