using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace TheTunnel
{
	public class AskCord<Tanswer,Tquestion>: IAskCord<Tanswer,Tquestion>
	{
		public AskCord(int OUTCid, ISerializer serializer, IDeserializer<Tanswer> deserializer)
		{
			awaitingQueue = new Dictionary<int, answerAwaiter<Tanswer>> ();
			this.OUTCid = (short)OUTCid;
			this.Serializer = serializer;
			this.SerializerT = serializer as ISerializer<Tquestion>;
			this.Deserializer = deserializer;
			this.DeserializerT = deserializer;
			MaxAwaitMs = 10000;
		}

		int id = 0;

		public event Action<IOutCord, MemoryStream, int> NeedSend;

		public short OUTCid { get;	protected set; }

		public ISerializer Serializer {	get; protected set; }

		ISerializer<Tquestion> SerializerT;
	
		public IDeserializer Deserializer {get; protected set; }

		IDeserializer<Tanswer> DeserializerT;

		public Tanswer AskT (Tquestion question)
		{
			Tanswer answer;

			MemoryStream str = new MemoryStream ();

			str.WriteByte((byte)(OUTCid & 255));
			str.WriteByte((byte)(OUTCid >> 8));

			var i = Interlocked.Increment (ref id);

			str.WriteByte((byte)(i & 255));
			str.WriteByte ((byte)(i >> 8));

			SerializerT.SerializeT (question, str);

			var aa = new answerAwaiter<Tanswer> (); 

			lock(awaitingQueue) {
				awaitingQueue.Add (id,aa);
			}

			str.Position = 0;
			if (NeedSend != null)
				NeedSend (this, str, (int)str.Length);

			var hasAns = aa.mre.WaitOne (MaxAwaitMs);
			if (hasAns)
				answer= aa.ans;
			else {
				answer = default(Tanswer);
			
				lock(awaitingQueue) {
					awaitingQueue.Remove (id);
				}
			}
			return answer;
		}
			
		public object Ask (object question)
		{
			return AskT ((Tquestion)question);
		}
			
		Dictionary<int, answerAwaiter<Tanswer>> awaitingQueue; 

		void answerCord_OnAnswer (ushort id, Tanswer answer)
		{
			answerAwaiter<Tanswer> aa = null;

			lock(awaitingQueue) {
				if (awaitingQueue.TryGetValue (id, out aa))
					awaitingQueue.Remove (id);
			}

			if (aa != null) {
				aa.ans = answer;
				aa.mre.Set ();
			}
		}

		public event Action<IInCord, object> OnReceive;

		byte[] idBuff = new byte[2];
		public void Parse (System.IO.MemoryStream stream)
		{
			stream.Read (idBuff, 0, 2);
			var id = BitConverter.ToUInt16 (idBuff, 0);
			var obj = DeserializerT.DeserializeT (stream, (int)(stream.Length - stream.Position));
			if (OnReceive != null)
				OnReceive (this, obj);
			answerCord_OnAnswer (id, obj);

		}

		public int MaxAwaitMs { get; set; }

		public short INCid {
			get { return (short)-OUTCid; }
		}

	}

	class answerAwaiter <Tanswer>
	{
		public answerAwaiter()
		{
			mre = new ManualResetEvent (false);
		}
		public ManualResetEvent mre;
		public Tanswer ans;
	}
}

