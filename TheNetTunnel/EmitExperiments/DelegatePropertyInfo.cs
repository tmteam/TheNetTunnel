using System;
using System.Reflection;

namespace EmitExperiments
{
    public class DelegatePropertyInfo
    {
        public MethodInfo DelegateInvokeMethodInfo { get; set; }
        public Type[] ParameterTypes { get; set; }
        public Type ReturnType { get; set; }

    }
}