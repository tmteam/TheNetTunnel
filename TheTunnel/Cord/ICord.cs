using System;

namespace TheTunnel
{
	public interface IOutCord
	{
		short Cid{get;}
		ISerializer Serializer{get;}
		event Action<IOutCord, byte[]> NeedSend;
	}
	public interface IOutCord<T>:IOutCord
	{
		ISerializer<T> SerializerT{get;}
		void Send(T obj);
	}

	public interface IInCord
	{	short Cid{get;}
		IDeserializer Deserializer{get;}
		bool Parse(byte[] msg, int offset);
	    event Action<IInCord, object> OnReceive;
	}

	public interface IInCord<T>:IInCord
	{	IDeserializer<T> DeserializerT{get;}
		event Action<IInCord, T> OnReceiveT;
	}

	public interface IAnswerCord:IInCord
	{
		IOutCord AnsweringCord{get;}
		void Answer(object val);
	}

	public interface IAskCord: IOutCord
	{
		IInCord ReceiveCord{get;}
		object Ask(object question);
	}

	public interface IAskCord<TAnswer, TQuestion>: IAskCord
	{
		TAnswer AskT(TQuestion question);
	}
}

