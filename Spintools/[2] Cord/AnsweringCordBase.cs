using System;

namespace TheTunnelOld
{
	public abstract class AnsweringCordBase<Tanswer>: CordBase 
	{
		public AnsweringCordBase (string name) : base (name){}

		public override bool Handle (byte[] qMsg)
		{
			var id = BitConverter.ToUInt16 (qMsg, 4);
			Tanswer ans;

			if (TryDeserialize (qMsg, 6, out ans)) {
				if (OnAnswer != null)
					OnAnswer (this, id, ans);
				return true;
			}
			else
				return false;
		}

		public void SendAnswer(Tanswer ans, UInt16 id)
		{
			var res = Serialize (ans, 6);
			BName.CopyTo (res, 0);
			res [4] = (byte) (id & 255);
			res [5] = (byte) (id >> 8);
			RaiseNeedSend (res);
		}

		protected abstract byte[] Serialize(Tanswer val, int valOffset);
		protected abstract bool TryDeserialize(byte[] qMsg, int offset,out Tanswer answer);

		public delegate void delOnCordAnswer(object sender, UInt16 id, Tanswer answer); 
		public event delOnCordAnswer OnAnswer;
	}

}

