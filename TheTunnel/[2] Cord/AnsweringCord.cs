using System;

namespace TheTunnel
{
	public class AnsweringCord:IAnsweringCord
	{
		public AnsweringCord(short cid, IDeserializer qDeserializer, ISerializer aSerializer )
		{
			this.INCid = cid;
			this.Deserializer = qDeserializer;
			this.Serializer = aSerializer;
		}

		public void Answer (object val, ushort id)
		{
			byte[] qmsg;
			if (!Serializer.TrySerialize (val, 4, out qmsg)) {
				return;
			}
			qmsg [0] = (byte)(OUTCid & 255);
			qmsg [1] = (byte)(OUTCid >> 8);
			qmsg [2] = (byte)(id & 255);
			qmsg [3] = (byte)(id >> 8);
			if (NeedSend != null)
				NeedSend (this, qmsg);
		}

		public event Action<IOutCord, byte[]> NeedSend;

		public void Send (object obj)
		{
			throw new NotImplementedException ();
		}

		public short OUTCid {
			get {return (short)-INCid; }
		}

		public ISerializer Serializer {	get ; protected set; }

		public short INCid { get; protected set; }

		public event Action<IAnsweringCord, ushort, object> OnAsk;

		public event Action<IInCord, object> OnReceive;

		public bool Parse (byte[] msg, int offset)
		{
			object Q = null;
			if (Deserializer.TryDeserialize (msg, offset+2, out Q)) {
				if (OnReceive != null)
					OnReceive (this, Q);
				ushort id = (ushort)(msg [offset] + (msg [offset + 1] << 8));
				if (OnAsk != null)
					OnAsk (this, id, Q);
				return true;
			}
			return false;
		}

		public short Cid { get;	protected set;}

		public IDeserializer Deserializer {	get; protected set;	}

	}
}

