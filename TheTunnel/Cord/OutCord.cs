using System;

namespace TheTunnel
{
	public class OutCord<T>: IOutCord<T>
	{
		public OutCord(short cid,ISerializer serializer)
		{
			this.OUTCid = cid;
			this.Serializer = serializer;
			this.SerializerT = serializer as ISerializer<T>;
		}

		public event Action<IOutCord, byte[]> NeedSend;

		public short OUTCid { get;	protected set; }

		public ISerializer Serializer {	get; protected set;	}

		public void Send (T obj)
		{
			byte[] res = null;
			if (Serializer.TrySerialize (obj, 2, out res)) {
				res [0] = (byte)(OUTCid & 255);
				res [1] = (byte)(OUTCid >> 8);
				if (NeedSend != null)
					NeedSend (this, res);
			}
		}

		public ISerializer<T> SerializerT {	get ; protected set; }
	}
}

