using System;

namespace TheTunnel
{
	public interface ISayingCord
	{
		string Name{ get; }
		byte[] BName{get;}
		bool Parse (byte[] qMsg);
		event Action<ISayingCord,byte[]> NeedSend;
	}

	public interface ISayingCord<Tmsg>: ISayingCord
	{
		void Send(Tmsg msg);
		event Action<ISayingCord<Tmsg>, Tmsg> OnReceive;
	}

	public interface IAskingCord: ISayingCord
	{
		ISayingCord AnswerCord{ get; }
	}

	public interface IAskingCord<Tquestion,Tanswer>: IAskingCord, ISayingCord<Tquestion> where Tanswer: class 
	{
		new ISayingCord<Tanswer> AnswerCord {get;}
		Tanswer Ask(Tquestion question, int maxAwaitTime);
	}

}

