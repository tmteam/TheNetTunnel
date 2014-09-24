using System;

namespace TheTunnel
{
	public abstract class DeserializerBase<T>:IDeserializer<T>
	{
		public int? Size{ get; protected set;}

		public abstract bool TryDeserializeT (byte[] arr, int offset, out T obj, int length = -1);

		public virtual bool TryDeserialize (byte[] arr, int offset, out object obj, int length = -1)
		{
			T des;
			var res = TryDeserializeT (arr, offset, out des, length);
			obj = des;
			return res;
		}
	}
}

