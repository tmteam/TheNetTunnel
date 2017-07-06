using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace TNT.Deserializers
{
	public static class DeserializersFactory
	{
		public static IDeserializer Create(Type t)
	{
		
		if (t == typeof(string))
			return new UnicodeDeserializer ();
		
		if (t == typeof(DateTime))
			return new UTCFileTimeDeserializer ();
		
		if (t.GetCustomAttributes (true).Any (a => a is ProtoBuf.ProtoContractAttribute)) {
			var gt =typeof(ProtoDeserializer<>).MakeGenericType (t);
			return Activator.CreateInstance (gt) as IDeserializer;
		}
		else if (t.IsArray) {	
			var gt = typeof(ArrayDeserializer<>).MakeGenericType (t);
			return Activator.CreateInstance (gt) as IDeserializer;
		}
		else if (t.IsClass && (t.StructLayoutAttribute.Pack!=1 || (t.StructLayoutAttribute.Value== LayoutKind.Auto)))
			throw new ArgumentException("Type "+ t.Name+" cannot be deserialized. "
				+"Use protobuf deserialization with [ProtoContractAttribute] in case of complex type "
				+"or  [StructLayoutAttribute(LayoutKind.Explicit, Pack = 1)] // [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]  in case of fixed-size type");
		else if (t.IsEnum) {
				var gt = typeof(PrimitiveConverterDeserializer<,>).MakeGenericType (t, Enum.GetUnderlyingType (t));
				return Activator.CreateInstance (gt) as IDeserializer;
		}
		else {
			var gt =typeof(PrimitiveDeserializer<>).MakeGenericType (t);
			return Activator.CreateInstance (gt) as IDeserializer;
		}
	}
		public static IDeserializer Create(Type[] t)
		{
			if (t.Length == 1)
				return Create (t [0]);
			else
				return new SequenceDeserializer (t);
		}
	}
}

