using System;

namespace TheTunnel
{
	public interface IOutCord
	{
		short OUTCid{get;}
		ISerializer Serializer{get;}
		event Action<IOutCord, byte[]> NeedSend;
	}
	public interface IOutCord<T>:IOutCord
	{
		ISerializer<T> SerializerT{get;}
		void Send(T obj);
	}

	public interface IInCord
	{	short INCid{get;}
		IDeserializer Deserializer{get;}
		bool Parse(byte[] msg, int offset);
	    event Action<IInCord, object> OnReceive;
	}

	public interface IInCord<T>:IInCord
	{	IDeserializer<T> DeserializerT{get;}
		event Action<IInCord, T> OnReceiveT;
	}

	public interface IAnsweringCord:IInCord, IOutCord
	{
		void Answer(object val, ushort id);
		event Action<IAnsweringCord, ushort, object> OnAsk;
	}

	public interface IAskCord: IOutCord,IInCord
	{
		object Ask(object question);
	}

	public interface IAskCord<TAnswer, TQuestion>: IAskCord
	{
		TAnswer AskT(TQuestion question);
	}
}

