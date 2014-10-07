using System;
using System.IO;

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

		public event Action<IOutCord, MemoryStream,int> NeedSend;

		public short OUTCid { get;	protected set; }

		public ISerializer Serializer {	get; protected set;	}

		public ISerializer<T> SerializerT {	get ; protected set; }

		public void Send (T obj)
		{
			MemoryStream stream = new MemoryStream ();
			stream.WriteByte ((byte)(OUTCid & 255));
			stream.WriteByte ((byte)(OUTCid >> 8));

			SerializerT.SerializeT (obj, stream);

			var sPos = (int)stream.Position;
			stream.Position = 0;

			if (NeedSend != null)
				NeedSend (this, stream,sPos);
		}

	}
}

