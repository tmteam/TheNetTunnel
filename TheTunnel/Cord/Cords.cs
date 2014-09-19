using System;

namespace TheTunnel
{
	public class OutCord<T>: IOutCord<T>
	{
		public OutCord(short cid,ISerializer serializer)
		{
			this.Cid = cid;
			this.Serializer = serializer;
			this.SerializerT = serializer as ISerializer<T>;
		}

		public event Action<IOutCord, byte[]> NeedSend;

		public short Cid { get;	protected set; }

		public ISerializer Serializer {	get; protected set;	}

		public void Send (T obj)
		{
			byte[] res = null;
			if (Serializer.TrySerialize (obj, 2, out res)) {
				res [0] = (byte)(Cid & 255);
				res [1] = (byte)(Cid >> 8);
				if (NeedSend != null)
					NeedSend (this, res);
			}
		}

		public ISerializer<T> SerializerT {	get ; protected set; }
	}

	public class AskCord<Tanswer,Tquestion>: IAskCord<Tanswer,Tquestion>
	{
		public AskCord(short Cid, ISerializer serializer, IDeserializer<Tanswer> deserializer)
		{
			this.Cid = Cid;
			this.Serializer = serializer;
			var crd = new  InCord<Tanswer> (-Cid, deserializer);
			this.ReceiveCord = crd;

			crd.OnReceiveT+= OnAnswerReceived;
		}



		public AskCord(IInCord<Tanswer> Receivecord, ISerializer serializer)
		{
			this.ReceiveCord = Receivecord;
			this.Serializer = serializer;
			this.Cid = -ReceiveCord.Cid;

			Receivecord.OnReceiveT += OnAnswerReceived;
		}
		#region IOutCord implementation

		public event Action<IOutCord, byte[]> NeedSend;

		public short Cid { get;	protected set; }

		public ISerializer Serializer {	get; protected set; }

		#endregion

		#region IAskCord implementation
		public Tanswer AskT (Tquestion question)
		{
			byte[] res = null;
			if (Serializer.TrySerialize (question, 2, out res)) {
				res [0] = (byte)(Cid & 255);
				res [1] = (byte)(Cid >> 8);
				if (NeedSend != null)
					NeedSend (this, res);
			}
			//!!!
			return default(Tanswer);
		}
			
		#endregion
		#region IAskCord implementation

		public object Ask (object question)
		{
			return AskT ((Tquestion)question);
		}

		public IInCord ReceiveCord {get; protected set;	}


		void OnAnswerReceived (IInCord incord, Tanswer ans)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}

	public class InCord<T> : IInCord<T>
	{
		public InCord(short cid, IDeserializer deserializer)
		{
			this.Cid = cid;
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
			if (Deserializer.TryDeserialize (msg, offset, res)) {
				if (OnReceiveT != null)
					OnReceiveT (this, res);
				if (OnReceive != null)
					OnReceive (this, res);
				return true;
			}
			return false;
		}

		public short Cid { get;	protected set;}

		public IDeserializer Deserializer {	get; protected set;	}
		#endregion
	}

	public class AnswerCord:IAnswerCord
	{
		public AnswerCord(short cid, IDeserializer deserializer)
		{
			this.Cid = cid;
			this.Deserializer = deserializer;
			this.DeserializerT = deserializer as IDeserializer<T>;
		}

		public void Answer (object val)
		{
			throw new NotImplementedException ();
		}


		public IOutCord AnsweringCord {
			get {
				throw new NotImplementedException ();
			}
		}


		#region IInCord implementation

		public event Action<IInCord, object> OnReceive;

		public bool Parse (byte[] msg, int offset)
		{

		}

		public short Cid { get;	protected set;}

		public IDeserializer Deserializer {	get; protected set;	}
		#endregion
	}
}

