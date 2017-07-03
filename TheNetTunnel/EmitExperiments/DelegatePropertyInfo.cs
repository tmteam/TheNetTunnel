using System;
using System.Reflection;

namespace EmitExperiments
{
    class DelegatePropertyInfo
    {
        public MethodInfo HandleDelegateGetMethodInfo { get; set; }
        public MethodInfo DelegateInvokeMethodInfo { get; set; }
        public Type[] ParameterTypes { get; set; }
        public Type ReturnType { get; set; }

    }
}