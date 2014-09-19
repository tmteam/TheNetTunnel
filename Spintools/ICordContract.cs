using System;

namespace TheTunnel
{

	public interface ICordContract
	{
		void OnDisconnect(DisconnectCause сause);
		event Action<ICordContract> PleaseDisconnect;

	}

	public enum DisconnectCause
	{
		ByContract,
		NoAnswer,
		Forcibly,
	}

	[AttributeUsage( AttributeTargets.Method)]
	public class ReceiveAttribute: Attribute  { public ReceiveAttribute(string cordName){ this.CordName = cordName;} public readonly string CordName;}

	[AttributeUsage( AttributeTargets.Method)]
	public class AnswerAttribute: Attribute  
	{ public AnswerAttribute(string questionCordName, string answerCordName)
		{ 	this.QuestionCordName = questionCordName;
			this.AnswerCordName = answerCordName;} 
		public readonly string QuestionCordName;
		public readonly string AnswerCordName;
	}

	[AttributeUsage( AttributeTargets.Property)]
	public class SendAttribute: Attribute { public SendAttribute(string cordName){ this.CordName = cordName;} public readonly string CordName;}

	[AttributeUsage( AttributeTargets.Property)]
	public class AskAttribute: Attribute 
	{ public AskAttribute(string questionCordName, string answerCordName)
		{ 	this.QuestionCordName = questionCordName;
			this.AnswerCordName = answerCordName;} 
		public readonly string QuestionCordName;
		public readonly string AnswerCordName;
	}

}


