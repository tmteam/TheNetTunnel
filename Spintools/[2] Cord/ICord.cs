using System;

namespace TheTunnelOld
{
	/// <summary>
	/// Some message-type(cord) handler
	/// </summary>
	public interface ICord
	{
		/// <summary>
		/// Cord(message-type)name
		/// </summary>
		/// <value>Lenght must be equal 4. Use only ASCII Symbols</value>
		string Name{ get; }
		/// <summary>
		/// Cord(message-type)name in bytes presentation
		/// </summary>
		/// <value>Lenght must be equal 4</value>
		byte[] BName{ get; }
		/// <summary>
		/// Handling byte message
		/// </summary>
		/// <param name="qMsg">raw message</param>
		bool Handle (byte[] qMsg);
		/// <summary>
		/// Occurs when the need 2 send byte message.
		/// </summary>
		event Action<ICord,byte[]> Need2Send;
	}
	public interface ISayingCord: ICord
	{
		event Action<ISayingCord, object> OnReceive;
	}
	/// <summary>
	/// Cord, that can convert and send typed message
	/// </summary>
	public interface ISayingCord<Tmsg>: ISayingCord
	{
		/// <summary>
		/// Send the specified typed message.
		/// </summary>
		/// <param name="msg">Message.</param>
		void Send(Tmsg msg);
	}
	/// <summary>
	/// Cord that can wait for a response message after sending
	/// </summary>
	public interface IAskingCord: ICord
	{
		/// <summary>
		/// Cord that handles response messages
		/// </summary>
		/// <value></value>
		ICord AnswerCord{ get; }
	}

	public interface IAskingCord<Tquestion,Tanswer>: IAskingCord
	{
		Tanswer Ask(Tquestion question, int maxAwaitTimeInMs);
		bool TryAsk(Tquestion question, int maxAwaitTimeInMs, out Tanswer answer);
	}
}

