using System;
using System.Reflection;

namespace TNT.Presentation;

public static class PresentationHelper
{
    public static bool IsNulable(this Type type)
    {
        return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}