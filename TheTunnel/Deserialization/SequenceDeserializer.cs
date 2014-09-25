using System;

namespace TheTunnel
{
	public class SequenceDeserializer:IDeserializer<object[]>
	{
		public readonly Type[] Types;
		IDeserializer[] deserializers;
		public SequenceDeserializer (Type[] types){
			this.Types = types;
			deserializers = new IDeserializer[types.Length];
			for (int i = 0; i < types.Length; i++)
				deserializers [i] = DeserializersFactory.Create (types [i]);
			Size = null;
		}

		#region IDeserializer implementation

		public bool TryDeserializeT (byte[] arr, int offset, out object[] obj, int length = -1)
		{
			length = length == -1 ? arr.Length - offset : length;
			int pos = offset;
			obj = new object[deserializers.Length];
			int i = 0;
			foreach (var des in deserializers) {
				if (des.Size.HasValue) {
					if (arr.Length < pos + des.Size.Value)
						return false;
					if (des.TryDeserialize (arr, pos, out obj [i]))
						pos += des.Size.Value;
					else
						return false;
				} else {
					if (arr.Length < pos + 4)
						return false;
					var len = BitConverter.ToInt32 (arr, pos);
					if (arr.Length < pos + 4 + len)
						return false;
					pos += 4;
					if (des.TryDeserialize (arr, pos, out obj [i], len))
						pos += len;
					else
						return false;
				}
				i++;
			}
			return true;
		}

		#endregion

		#region IDeserializer implementation

		public bool TryDeserialize (byte[] arr, int offset, out object obj, int length = -1)
		{
			object[] ret;
			var res = TryDeserializeT (arr, offset, out ret, length);
			if (res)
				obj = ret;
			else
				obj = null;
			return res;
		}

		public int? Size { get; protected set;	}

		#endregion
	}
}

