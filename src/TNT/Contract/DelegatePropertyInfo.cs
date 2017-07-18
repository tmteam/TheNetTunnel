using System;
using System.Reflection;

namespace TNT.Presentation
{
    public class DelegatePropertyInfo
    {
        public MethodInfo DelegateInvokeMethodInfo { get; set; }
        public Type[] ParameterTypes { get; set; }
        public Type ReturnType { get; set; }
    }
}