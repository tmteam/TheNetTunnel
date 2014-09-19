using System;

namespace TheTunnel
{
	public abstract class SayingCordBase<Tmsg>:  CordBase,ISayingCord<Tmsg>{
		public SayingCordBase (string cordName) : base (cordName){}

		protected void raiseOnReceive(Tmsg msg)	
		{
			if (OnReceiveT != null)
				OnReceiveT (this, msg);
			if (OnReceive != null)
				OnReceive (this, msg);
		}

		public virtual void Send (Tmsg msg)
		{
			var resa = Serialize (msg, 4);
			this.BName.CopyTo (resa, 0);
			RaiseNeedSend(resa);
		}

		public override bool Handle(byte[] qMsg)
		{
			if (qMsg.Length < 4)
				return false;
			Tmsg res;
			if(TryDeserialize (qMsg, 4, out res))
			{
				raiseOnReceive (res);
				return true;
			}
			else
				return false;
		}

		protected abstract byte[] Serialize(Tmsg msg, int valOffset);
		protected abstract bool TryDeserialize(byte[] qMsg, int offset, out Tmsg msg);
		/// <summary>
		/// Occurs on new message receiving.
		/// </summary>
		public event Action<ISayingCord<Tmsg>, Tmsg> OnReceiveT;
		public event Action<ISayingCord, object> OnReceive;
	}
}

