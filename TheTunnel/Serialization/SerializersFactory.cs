using System;
using System.Linq;
using System.Runtime.InteropServices;


namespace TheTunnel
{
	public static class SerializersFactory
	{

		public static ISerializer GetSerializer(Type t)
		{
			if (t == typeof(string))
				return new UnicodeSerializer ();
			if (t == typeof(DateTime))
				return new UTCFileTimeSerializer ();
			if (t.GetCustomAttributes (true).Any (a => a is ProtoBuf.ProtoContractAttribute))
				return new ProtoSerializer ();
			else if (t.IsArray) {	
				var gt = typeof(ArraySerializer<>).MakeGenericType (t);
				return Activator.CreateInstance (gt) as ISerializer;
			}
			else if (t.IsClass 
				&& !t.GetCustomAttributes(true)
					.Any(a=>a.GetType() == typeof(StructLayoutAttribute) 
						&& (a as StructLayoutAttribute).Value == LayoutKind.Explicit)) {
				throw new ArgumentException("Type "+ t.Name+" cannot be serialized. Use protobuf serialization with [ProtoContractAttribute] in case of complex class or  [StructLayoutAttribute(Value = LayoutKind.Explicit)] in case of fixed-size types");
			}
			else
			{
				var gt =typeof(PrimitiveSerializer<>).MakeGenericType (t);
				return Activator.CreateInstance (gt) as ISerializer;
			}
		}
		public static IDeserializer GetDeserializer(Type t)
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
			else if (t.IsClass 
				&& !t.GetCustomAttributes(true)
					.Any(a=>a.GetType() == typeof(StructLayoutAttribute) 
						&& (a as StructLayoutAttribute).Value == LayoutKind.Explicit)) {
				throw new ArgumentException("Type "+ t.Name+" cannot be deserialized. Use protobuf deserialization with [ProtoContractAttribute] in case of complex type or  [StructLayoutAttribute(Value = LayoutKind.Explicit)] in case of fixed-size type");
			}
			else {
				var gt =typeof(PrimitiveDeserializer<>).MakeGenericType (t);
				return Activator.CreateInstance (gt) as IDeserializer;
			}
		}
	}
}

