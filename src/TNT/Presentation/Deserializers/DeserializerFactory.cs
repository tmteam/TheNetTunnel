using System;
using System.Collections.Generic;
using System.Linq;
using TNT.Exceptions.ContractImplementation;

namespace TNT.Presentation.Deserializers
{
    public class DeserializerFactory
    {
        public static IDeserializer CreateProtoDeserializer(Type protoContractType)
        {
            var gt = typeof(ProtoDeserializer<>).MakeGenericType(protoContractType);
            return Activator.CreateInstance(gt) as IDeserializer;
        }

        public static IDeserializer CreateArrayDeserializer(Type arrayType, DeserializerFactory factory)
        {
            var gt = typeof(ArrayDeserializer<>).MakeGenericType(arrayType);
            return Activator.CreateInstance(gt, factory) as IDeserializer;
        }

        public static IDeserializer CreateEnumDeserializer(Type enumType)
        {
            var gt = typeof(EnumDeserializer<>).MakeGenericType(enumType);
            return Activator.CreateInstance(gt) as IDeserializer;
        }

        public static IDeserializer CreateDotNetValueTypeSerializer(Type valueType)
        {
            var gt = typeof(ValueTypeDeserializer<>).MakeGenericType(valueType);
            return Activator.CreateInstance(gt) as IDeserializer;
        }
        
        public static DeserializerFactory CreateDefault(params DeserializationRule[] additionalRules)
        {
            var ans = new DeserializerFactory();
            foreach (var serializationRule in additionalRules)
            {
                ans.AddRule(serializationRule);
            }
            ans.AddRule(DeserializationRule.Create(new UnicodeDeserializer()));
            ans.AddRule(DeserializationRule.Create(new UTCFileTimeDeserializer()));
            ans.AddRule(DeserializationRule.Create(new UTCFileTimeAndOffsetDeserializer()));
            ans.AddRule(new DeserializationRule(
                t => Attribute.IsDefined(t, typeof(ProtoBuf.ProtoContractAttribute)), CreateProtoDeserializer));
            ans.AddRule(new DeserializationRule(t=>t.IsArray, CreateArrayDeserializer));
            ans.AddRule(new DeserializationRule(t=>t.IsEnum, CreateEnumDeserializer));
            ans.AddRule(new DeserializationRule(t=>t.IsValueType, CreateDotNetValueTypeSerializer));
            return ans;
        }

        private List<DeserializationRule> _rules = new List<DeserializationRule>();
        public void AddRule(DeserializationRule rule)
        {
            _rules.Add(rule);
        }
           
        public  IDeserializer Create(Type t)
        {
            foreach (var rule in _rules)
            {
                if (rule.Rule(t))
                    return rule.GetDeserializer(t, this);
            }
            throw new TypeCannotBeDeserializedException(t);
        }
        public IDeserializer Create(Type[] t)
        {
            if (t.Length == 1)
                return Create(t[0]);
            else
                return new SequenceDeserializer(t.Select(Create).ToArray());
        }
    }
}
