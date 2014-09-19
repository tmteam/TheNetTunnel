using System;

namespace TheTunnelOld
{
	public class AnsweringProtoCord<Tanswer>: AnsweringCordBase<Tanswer>
	{
		public AnsweringProtoCord (string cordName): base(cordName)
		{
		}

		#region implemented abstract members of AnsweringCordBase

		protected override byte[] Serialize (Tanswer val, int valOffset)
		{
			return ProtoTools.Serialize (val, valOffset);
		}

		protected override bool TryDeserialize (byte[] qMsg, int offset, out Tanswer answer)
		{
			return ProtoTools.TryDeserialize (qMsg, offset, out answer);
		}

		#endregion
	}
}

