using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace TheTunnel.Serialization
{
	public static class SerializersFactory
	{
		public static ISerializer Create(Type t){
			if (t == typeof(string))
				return new UnicodeSerializer ();

			if (t == typeof(DateTime))
				return new UTCFileTimeSerializer ();

			if (t.GetCustomAttributes (true).Any (a => a is ProtoBuf.ProtoContractAttribute)) {
				var gt = typeof(ProtoSerializer<>).MakeGenericType (t);
				return Activator.CreateInstance (gt) as ISerializer;
			}
			else if (t.IsArray) {	
				var gt = typeof(ArraySerializer<>).MakeGenericType (t);
				return Activator.CreateInstance (gt) as ISerializer;
			}
			else if (t.IsClass && (t.StructLayoutAttribute.Pack != 1 || (t.StructLayoutAttribute.Value == LayoutKind.Auto)))
				throw new ArgumentException ("Type " + t.Name + " cannot be serialized. "
				+ "Use protobuf serialization with [ProtoContractAttribute] in case of complex type "
				+ "or  [StructLayoutAttribute(LayoutKind.Explicit, Pack = 1)] // [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]  in case of fixed-size type");
			else if (t.IsEnum) {
				var underT = Enum.GetUnderlyingType (t);
				var gt =typeof(PrimitiveConvertSerializer<,>).MakeGenericType (t,underT);
				return Activator.CreateInstance (gt) as ISerializer;
			}
			else{
				var gt =typeof(PrimitiveSerializer<>).MakeGenericType (t);
				return Activator.CreateInstance (gt) as ISerializer;
			}
		}
		public static ISerializer Create(Type[] t)
		{
			//if(t.Length==0)
			//	return new PrimitiveSerializer<void>();
			if (t.Length == 1)
				return Create (t [0]);
			else
				return new SequenceSerializer (t);
		}
	}
}

