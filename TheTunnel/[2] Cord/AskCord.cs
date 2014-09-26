using System;
using System.Collections.Generic;
using System.Threading;

namespace TheTunnel
{
	public class AskCord<Tanswer,Tquestion>: IAskCord<Tanswer,Tquestion>
	{
		public AskCord(int OUTCid, ISerializer serializer, IDeserializer<Tanswer> deserializer)
		{
			awaitingQueue = new Dictionary<int, answerAwaiter<Tanswer>> ();
			this.OUTCid = (short)OUTCid;
			this.Serializer = serializer;
			this.Deserializer = deserializer;
		}

		int id = 0;

		#region IOutCord implementation

		public event Action<IOutCord, byte[]> NeedSend;

		public short OUTCid { get;	protected set; }

		public ISerializer Serializer {	get; protected set; }

		#endregion

		#region IAskCord implementation
		public Tanswer AskT (Tquestion question)
		{
			Tanswer answer = default(Tanswer);

			byte[] res = null;
			if (Serializer.TrySerialize (question, 4, out res)) {

				res [0] = (byte)(OUTCid & 255);
				res [1] = (byte)(OUTCid >> 8);

				Interlocked.Increment (ref id);

				res [2] = (byte)(id & 255);
				res [3] = (byte)(id>>8);

				var aa = new answerAwaiter<Tanswer> (); 
				lock(awaitingQueue) {
					awaitingQueue.Add (id,aa);
				}

				if (NeedSend != null)
					NeedSend (this, res);


				var hasAns = aa.mre.WaitOne (1000);
				if (hasAns)
					answer= aa.ans;
				else {
					answer = default(Tanswer);
					lock(awaitingQueue) {
						awaitingQueue.Remove (id);
					}
				}
			}

			return answer;
		}

		#endregion
		#region IAskCord implementation

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
		#endregion

		public event Action<IInCord, object> OnReceive;

		public bool Parse (byte[] msg, int offset)
		{
			object Q = null;
			if (Deserializer.TryDeserialize (msg, offset+2, out Q)) {
				if (OnReceive != null)
					OnReceive (this, Q);
				ushort id = (ushort)(msg [offset] + (msg [offset+1] << 8));
				answerCord_OnAnswer (id, (Tanswer)Q);
				return true;
			}
			return false;
		}

		public short INCid {
			get { return (short)-OUTCid; }
		}

		public IDeserializer Deserializer {
			get ;
			protected set;
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

