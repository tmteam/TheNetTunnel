using System;

namespace TheTunnelOld
{
	public class AskingProtocord<Tquestion, Tanswer>: AskingCordBase<Tquestion,Tanswer>
	{
		public AskingProtocord (string cordName, string answerCordName): base(cordName, answerCordName)
		{
		}

		#region implemented abstract members of AskingCordBase

		protected override Tanswer Answer (Tquestion question)
		{
			if (OnAsking != null)
				return OnAsking (this, question);
			else
				return default(Tanswer);
		}

		protected override AnsweringCordBase<Tanswer> CreateAnswerCord (string answerCordName)
		{
			return new AnsweringProtoCord<Tanswer> (answerCordName);
		}

		protected override byte[] Serialize (Tquestion question, int questionOffset)
		{
			return ProtoTools.Serialize (question, questionOffset);
		}

		protected override bool TryDeserialize (byte[] qMsg, int offset, out Tquestion question)
		{
			return ProtoTools.TryDeserialize (qMsg, offset, out question);
		}
		#endregion

		public event Func<ICord, Tquestion, Tanswer> OnAsking;
	}
}

