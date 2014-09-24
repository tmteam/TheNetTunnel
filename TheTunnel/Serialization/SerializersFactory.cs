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
			else if (t.IsClass && (t.StructLayoutAttribute.Pack!=1 || (t.StructLayoutAttribute.Value== LayoutKind.Auto)))
				throw new ArgumentException("Type "+ t.Name+" cannot be serialized. "
					+"Use protobuf serialization with [ProtoContractAttribute] in case of complex type "
					+"or  [StructLayoutAttribute(LayoutKind.Explicit, Pack = 1)] // [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]  in case of fixed-size type");
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
			else if (t.IsClass && (t.StructLayoutAttribute.Pack!=1 || (t.StructLayoutAttribute.Value== LayoutKind.Auto)))
				throw new ArgumentException("Type "+ t.Name+" cannot be deserialized. "
					+"Use protobuf deserialization with [ProtoContractAttribute] in case of complex type "
					+"or  [StructLayoutAttribute(LayoutKind.Explicit, Pack = 1)] // [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]  in case of fixed-size type");
			else {
				var gt =typeof(PrimitiveDeserializer<>).MakeGenericType (t);
				return Activator.CreateInstance (gt) as IDeserializer;
			}
		}
	}
}

