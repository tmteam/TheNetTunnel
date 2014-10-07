using System;
using System.IO;

namespace TheTunnel
{
	public class AnsweringCord:IAnsweringCord
	{
		public AnsweringCord(short cid, IDeserializer qDeserializer, ISerializer aSerializer )
		{
			this.INCid = cid;
			this.OUTCid = (short)(-INCid);
			this.Deserializer = qDeserializer;
			this.Serializer = aSerializer;
		}

		public short OUTCid { get; protected set;}

		public short INCid { get; protected set; }

		public ISerializer Serializer {	get ; protected set; }

		public IDeserializer Deserializer {	get; protected set;	}

		public event Action<IOutCord, MemoryStream, int> NeedSend;

		public event Action<IAnsweringCord, ushort, object> OnAsk;

		public event Action<IInCord, object> OnReceive;

		public void SendAnswer (object answer, ushort questionId)
		{

			MemoryStream str = new MemoryStream ();
			str.WriteByte ((byte)(OUTCid & 255));
			str.WriteByte ((byte)(OUTCid >> 8));
			str.WriteByte ((byte)(questionId & 255));
			str.WriteByte ((byte)(questionId >> 8));

			Serializer.Serialize (answer, str);

			if (NeedSend != null)
				NeedSend (this, str, (int)str.Length);
		}

		byte[] buffId = new byte[2];
		public void Parse (MemoryStream stream)
		{
			stream.Read (buffId, 0, 2);
			var id = BitConverter.ToUInt16 (buffId, 0);
			var askObj = Deserializer.Deserialize (stream, (int)stream.Length - 2);
			if (OnReceive != null)
				OnReceive (this, askObj);
			if (OnAsk != null)
				OnAsk (this, id, askObj);
		}

		public void Send (object obj){
			throw new NotImplementedException ();
		}
	}
}

