using System;
using System.Text;
using System.Threading;

namespace TheTunnel
{
	public abstract class AskingCordBase<Tquestion,Tanswer>: 
		SayingCordBase<Tquestion>,
		IAskingCord<Tquestion, Tanswer> where Tanswer: class
	{
		public AskingCordBase()
		{
			this.AnswerCord = CreateAnswerCord ();
			this.OnReceive+= (ISayingCord<Tquestion> sender, Tquestion val) => 
			{
				var ans = Answer(val);
				this.AnswerCord.Send(ans);
			};
		}

		ISayingCord IAskingCord.AnswerCord{ get { return AnswerCord; } }
		ISayingCord<Tanswer> answerCord;
		public ISayingCord<Tanswer> AnswerCord {
			get{ return answerCord; }
			private set{
				if (answerCord != null)
					answerCord.OnReceive -= answerCord_HandleOnReceive;
				answerCord = value;
				if(answerCord!=null)
					answerCord.OnReceive+= answerCord_HandleOnReceive;
			}
		}

		ManualResetEvent mre = new ManualResetEvent(false);

		Tanswer lastAnswer;

		protected abstract Tanswer Answer(Tquestion question);

		protected abstract ISayingCord<Tanswer> CreateAnswerCord();

		public Tanswer Ask (Tquestion question, int maxAwaitTime)
		{
			lastAnswer = null;
			mre.Reset ();
			Send (question);
			if (mre.WaitOne(maxAwaitTime))
				return lastAnswer;
			else
				return null;
		}


		void answerCord_HandleOnReceive (ISayingCord<Tanswer> arg1, Tanswer arg2)
		{
			lastAnswer = arg2;
			mre.Set ();
		}
	}


	public abstract class SayingCordBase<Tmsg>:  SayingCordBase,ISayingCord<Tmsg>
	{
		protected void raiseOnReceive(Tmsg val)
		{
			if (OnReceive != null)
				OnReceive (this, val);
		}
		public event Action<ISayingCord<Tmsg>, Tmsg> OnReceive;
		public void Send (Tmsg val)
		{
			SendNeedSend (Serialize (val));
		}
		public override bool Parse(byte[] qMsg)
		{
			var res = Deserialize (qMsg);
			if (res != null)
			{
				raiseOnReceive (res);
				return true;
			}
			return false;
		}
		protected abstract byte[] Serialize(Tmsg val);
		protected abstract Tmsg Deserialize(byte[] qMsg);

	}

	public abstract class SayingCordBase: ISayingCord
	{
		public SayingCordBase()
		{
			Name = GetName ();
			if (Name.Length != 4)
				throw new ArgumentException ("Cord name's lenght must be equal 4");
			BName = Encoding.ASCII.GetBytes (Name);
		}
		protected abstract string GetName(); 

		#region ISayCord implementation

		protected void SendNeedSend(byte[] cordmsg)
		{
			if(NeedSend!=null)
				NeedSend(this, cordmsg);
		}

		public event Action<ISayingCord, byte[]> NeedSend;

		public abstract bool Parse (byte[] qMsg);

		public byte[] BName {get; private set;}

		public string Name { get; private set;}

		#endregion
	}


}

