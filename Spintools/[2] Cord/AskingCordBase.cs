
using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;


namespace TheTunnelOld
{
	public abstract class AskingCordBase<Tquestion,Tanswer>: 
		CordBase, IAskingCord<Tquestion, Tanswer>
	{
		int id = 0;
		public AskingCordBase (string cordName, string answerCordName) : base (cordName)
		{
			this.AnswerCord = CreateAnswerCord (answerCordName);
			this.AnswerCord.OnAnswer+= answerCord_OnAnswer;
			awaitingQueue = new Dictionary<int, answerAwaiter<Tanswer>> ();
		}

		ICord IAskingCord.AnswerCord{ get { return AnswerCord; } }

		public AnsweringCordBase<Tanswer> AnswerCord { get; private set; }

		Dictionary<int, answerAwaiter<Tanswer>> awaitingQueue; 

		public override bool Handle(byte[] qMsg){

			if (qMsg.Length < 6)
				return false;
		
			var id = BitConverter.ToUInt16 (qMsg, 4);
			Tquestion question;

			if (TryDeserialize (qMsg, 6, out question)) {
				var ans = Answer (question);
				AnswerCord.SendAnswer (ans, id);
				return true;
			} else
				return false;
		}

		public Tanswer Ask (Tquestion question, int maxAwaitTimeInMs)
		{
			Tanswer res;
			TryAsk (question, maxAwaitTimeInMs, out res);
			return res;
		}

		public bool TryAsk (Tquestion question, int maxAwaitTimeInMs, out Tanswer answer)
		{
			var res = Serialize (question, 6);

			Interlocked.Increment (ref id);

			BName.CopyTo (res,0);
			res [4] = (byte) (id & 255);
			res [5] = (byte) (id >> 8);

			var aa = new answerAwaiter<Tanswer> (); 
			lock(awaitingQueue) {
				awaitingQueue.Add (id,aa);
			}

			RaiseNeedSend (res);
			var hasAns = aa.mre.WaitOne (maxAwaitTimeInMs);
			if (hasAns)
				answer= aa.ans;
			else {
				answer = default(Tanswer);
				lock(awaitingQueue) {
					awaitingQueue.Remove (id);
				}
			}
			return hasAns;
		}

		protected abstract Tanswer Answer(Tquestion question);

		protected abstract AnsweringCordBase<Tanswer> CreateAnswerCord(string answerCordName);

		protected abstract byte[] Serialize(Tquestion question, int questionOffset);

		protected abstract bool TryDeserialize(byte[] qMsg, int offset, out Tquestion question);

		void answerCord_OnAnswer (object sender, ushort id, Tanswer answer)
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