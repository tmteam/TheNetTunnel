using System;
using System.IO;
using TNT.Deserialization;
using TNT.Serialization;

namespace TNT.Cords
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

		public event Action<IAnsweringCord, short, object> OnAsk;

		public event Action<IInCord, object> OnReceive;

        bool isStopped = false;
		
        public void SendAnswer (object answer, short questionId)
		{
            if (isStopped)
                return;
            byte[] bHeadBuff = new byte[4];
            short[] sHeadBuff = new short[2];

			MemoryStream str = new MemoryStream ();

			sHeadBuff[0] = OUTCid;
			sHeadBuff[1] = questionId;
			
			System.Buffer.BlockCopy(sHeadBuff,0,bHeadBuff,0,4);

			str.Write (bHeadBuff, 0, 4);

			Serializer.Serialize (answer, str);
			str.Position = 0;
			if (NeedSend != null && ! isStopped)
				NeedSend (this, str, (int)str.Length);
		}

		
		public void Parse (MemoryStream stream)
        {
            if (isStopped)
                return;
            byte[] buffId = new byte[2];
			stream.Read (buffId, 0, 2);
			var id = BitConverter.ToInt16 (buffId, 0);
			var askObj = Deserializer.Deserialize (stream, (int)(stream.Length - stream.Position));
			if (OnReceive != null && ! isStopped)
				OnReceive (this, askObj);
			if (OnAsk != null && !isStopped)
				OnAsk (this, id, askObj);
		}

		public void Send (object obj){
			throw new NotSupportedException ();
		}
        public void Stop() 
        {
            isStopped = true;
        }

	}
}

