using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace TNT.Cord.Serializers
{
    public static class SerializersFactory
    {
        public static ISerializer Create(Type type)
        {
            throw  new NotImplementedException();
            //if (type == typeof(string))
            //    return new UnicodeSerializer();

            //if (type == typeof(DateTime))
            //    return new UTCFileTimeSerializer();

            //if (type.GetCustomAttributes(true).Any(a => a is ProtoBuf.ProtoContractAttribute))
            //{
            //    var gt = typeof(ProtoSerializer<>).MakeGenericType(type);
            //    return Activator.CreateInstance(gt) as ISerializer;
            //}
            //else if (type.IsArray)
            //{
            //    var gt = typeof(ArraySerializer<>).MakeGenericType(type);
            //    return Activator.CreateInstance(gt) as ISerializer;
            //}
            //else if (type.IsClass && (type.StructLayoutAttribute.Pack != 1 || (type.StructLayoutAttribute.Value == LayoutKind.Auto)))
            //    throw new ArgumentException("Type " + type.Name + " cannot be serialized. "
            //                                +
            //                                "Use protobuf serialization with [ProtoContractAttribute] in case of complex type "
            //                                +
            //                                "or  [StructLayoutAttribute(LayoutKind.Explicit, Pack = 1)] // [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]  in case of fixed-size type");
            //else if (type.IsEnum)
            //{
                
            //    var underT = Enum.GetUnderlyingType(type);
            //    var gt = typeof(PrimitiveConvertSerializer<,>).MakeGenericType(type, underT);
            //    return Activator.CreateInstance(gt) as ISerializer;
            //}
            //else
            //{
            //    var gt = typeof(ValueTypeSerializer<>).MakeGenericType(type);
            //    return Activator.CreateInstance(gt) as ISerializer;
            //}
        }

        public static ISerializer Create(Type[] t)
        {
            return null;
            //if (t.Length == 1)
            //    return Create(t[0]);
            //else
            //    return new SequenceSerializer(t);
        }
    }
}

