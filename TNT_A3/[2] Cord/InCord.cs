using System;
using System.IO;

namespace TheTunnel
{
	public class InCord<T> : IInCord<T>
	{
		public InCord(short cid, IDeserializer deserializer)
		{
			this.INCid = cid;
			this.Deserializer = deserializer;
			this.DeserializerT = deserializer as IDeserializer<T>;
		}
		#region IInCord implementation

		public event Action<IInCord, T> OnReceiveT;

		public IDeserializer<T> DeserializerT {	get; protected set; }

		#endregion

		#region IInCord implementation

		public event Action<IInCord, object> OnReceive;

		public void Parse (MemoryStream stream)
		{
			var res = DeserializerT.DeserializeT (stream, (int)(stream.Length - stream.Position));
			if(OnReceiveT!=null)
				OnReceiveT(this,res);
			if(OnReceive!=null)
				OnReceive(this, res);
		}

		public short INCid { get;	protected set;}

		public IDeserializer Deserializer {	get; protected set;	}
		#endregion
	}
}

