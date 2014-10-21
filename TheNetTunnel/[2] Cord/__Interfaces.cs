using System;
using System.IO;
using System.Reflection;
using TheTunnel.Serialization;
using TheTunnel.Deserialization;

namespace TheTunnel.Cords
{
		public interface IOutCord
		{
			short OUTCid{get;}
			ISerializer Serializer{get;}
			event Action<IOutCord, MemoryStream, int> NeedSend;
            void Stop();
		}

		public interface IOutCord<T>:IOutCord
		{
			ISerializer<T> SerializerT{get;}
			void Send(T obj);
		}

		public interface IInCord
		{	short INCid{get;}
			IDeserializer Deserializer{get;}
			void Parse(MemoryStream stream);
			event Action<IInCord, object> OnReceive;
		}

		public interface IInCord<T>:IInCord
		{	IDeserializer<T> DeserializerT{get;}
			event Action<IInCord, T> OnReceiveT;
		}

		public interface IAnsweringCord:IInCord, IOutCord
		{
			void SendAnswer(object val, short id);
			event Action<IAnsweringCord, short, object> OnAsk;
		}

		public interface IAskCord: IOutCord,IInCord
		{
			int MaxAwaitMs{get;set;} 
			object Ask(object question);
            
		}

		public interface IAskCord<TAnswer, TQuestion>: IAskCord
		{
			TAnswer AskT(TQuestion question);
		}
}

