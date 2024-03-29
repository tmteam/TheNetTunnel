﻿using System;
using System.Collections.Generic;
using System.Linq;
using TNT.Exceptions.ContractImplementation;
using System.Reflection;

namespace TNT.Presentation.Serializers;

public class SerializerFactory
{
    public static ISerializer CreateProtoSerializer(Type protoContractType)
    {
        var gt = typeof(ProtoSerializer<>).MakeGenericType(protoContractType);
        return Activator.CreateInstance(gt) as ISerializer;
    }

    public static ISerializer CreateArraySerializer(Type arrayType, SerializerFactory factory)
    {
        var gt = typeof(ArraySerializer<>).MakeGenericType(arrayType);
        return Activator.CreateInstance(gt, factory) as ISerializer;
    }

    public static ISerializer CreateEnumSerializer(Type enumType)
    {
        var gt = typeof(EnumSerializer<>).MakeGenericType(enumType);
        return Activator.CreateInstance(gt) as ISerializer;
    }

    public static ISerializer CreateDotNetValueTypeSerializer(Type valueType)
    {
        var gt = typeof(ValueTypeSerializer<>).MakeGenericType(valueType);
        return Activator.CreateInstance(gt) as ISerializer;
    }
    private static ISerializer CreateDotNetNullableSerializer(Type valueType)
    {
        var gt = typeof(NullableSerializer<>).MakeGenericType(valueType.GenericTypeArguments.First());
        return Activator.CreateInstance(gt) as ISerializer;
    }
    public static SerializerFactory CreateDefault(params SerializationRule[] additionalRules)
    {
        var ans = new SerializerFactory();
        foreach (var serializationRule in additionalRules)
        {
            ans.AddRule(serializationRule);
        }
        ans.AddRule(SerializationRule.Create(new ByteArraySerializer()));
        ans.AddRule(SerializationRule.Create(new ByteEnumerableSerializer()));
        ans.AddRule(SerializationRule.Create(new UnicodeSerializer()));
        ans.AddRule(SerializationRule.Create(new UTCFileTimeSerializer()));
        ans.AddRule(SerializationRule.Create(new UTCFileTimeAndOffsetSerializer()));
        ans.AddRule(new SerializationRule(t => t.GetTypeInfo().IsDefined(typeof(ProtoBuf.ProtoContractAttribute)), CreateProtoSerializer));
        ans.AddRule(new SerializationRule(t => t.GetTypeInfo().IsArray, CreateArraySerializer));
        ans.AddRule(new SerializationRule(t => t.GetTypeInfo().IsEnum, CreateEnumSerializer));
        ans.AddRule(new SerializationRule(PresentationHelper.IsNulable, CreateDotNetNullableSerializer));
        ans.AddRule(new SerializationRule(t => t.GetTypeInfo().IsValueType, CreateDotNetValueTypeSerializer));
        return ans;
    }

      
    private readonly List<SerializationRule> _rules = new List<SerializationRule>();

    public void AddRule(SerializationRule rule)
    {
        _rules.Add(rule);
    }

    public ISerializer Create(Type t)
    {
        foreach (var rule in _rules)
        {
            if (rule.Rule(t))
                return rule.GetSerializer(t, this);
        }
        throw new TypeCannotBeSerializedException(t);

    }

    public ISerializer Create(Type[] t)
    {
        if (t.Length == 1)
            return Create(t[0]);
        else
            return new SequenceSerializer(t.Select(Create).ToArray());
    }
}