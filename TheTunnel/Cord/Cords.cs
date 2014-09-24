using System;
using System.Collections.Generic;
using System.Threading;

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
	class answerAwaiter <Tanswer>
	{
		public answerAwaiter()
		{
			mre = new ManualResetEvent (false);
		}
		public ManualResetEvent mre;
		public Tanswer ans;
	}
	public class AskCord<Tanswer,Tquestion>: IAskCord<Tanswer,Tquestion>
	{
		public AskCord(short OUTCid, ISerializer serializer, IDeserializer<Tanswer> deserializer)
		{
			awaitingQueue = new Dictionary<int, answerAwaiter<Tanswer>> ();
			this.OUTCid = OUTCid;
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

	public class InCord<T> : IInCord<T>
	{
		public InCord(short cid, IDeserializer deserializer)
		{
			this.INCid = cid;
			this.Deserializer = deserializer;
			this.DeserializerT = deserializer as IDeserializer<T>;
		}
		#region IInCord implementation

		public event Action<IInCord, T> OnReceiveT;

		public IDeserializer<T> DeserializerT {	get; protected set; }

		#endregion

		#region IInCord implementation

		public event Action<IInCord, object> OnReceive;

		public bool Parse (byte[] msg, int offset)
		{
			T res;
			object ores;
			if (Deserializer.TryDeserialize (msg, offset,out ores)) {
				res = (T)ores;
				if (OnReceiveT != null)
					OnReceiveT (this, res);
				if (OnReceive != null)
					OnReceive (this, res);
				return true;
			}
			return false;
		}

		public short INCid { get;	protected set;}

		public IDeserializer Deserializer {	get; protected set;	}
		#endregion
	}

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
			byte[] qmsg = Serializer.Serialize (val, 4);
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

