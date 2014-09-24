using System;

namespace TheTunnel
{
	public abstract class SerializerBase<T>:ISerializer<T>, ISerializer 
	{
		public int? Size{ get; protected set;}

		public virtual bool TrySerialize (T obj, int offset, out byte[] arr)
		{
			arr = Serialize (obj, offset);
			return true;
		}

		public abstract bool TrySerialize (T obj, byte[] arr, int offset);

		public abstract byte[] Serialize (T obj, int offset);

		bool ISerializer.TrySerialize (object obj, byte[] arr, int offset){
			return TrySerialize ((T)obj, arr, offset);
		}

		bool ISerializer.TrySerialize (object obj, int offset, out byte[] arr)
		{
			return TrySerialize ((T)obj, offset, out arr);
		}

		byte[] ISerializer.Serialize (object obj, int offset)
		{
			return Serialize ((T)obj, offset);
		}
	}

}

