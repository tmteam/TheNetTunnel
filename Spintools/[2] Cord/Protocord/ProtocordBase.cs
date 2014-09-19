
using System;
using ProtoBuf;
using System.IO;

namespace TheTunnelOld
{
	public class ProtocordBase<T>: SayingCordBase<T>
	{
		public ProtocordBase (string cordName): base(cordName)
		{
		}

		protected override byte[] Serialize (T msg, int valOffset)
		{
			return ProtoTools.Serialize (msg, valOffset);
		}

		protected override bool TryDeserialize (byte[] qMsg, int offset, out T msg)
		{
			return ProtoTools.TryDeserialize (qMsg, offset, out msg);
		}

	}
	public class ProtocordBase: SayingCordBase<object>
	{
		public ProtocordBase (string cordName): base(cordName)
		{
		}

		protected override byte[] Serialize (object msg, int valOffset)
		{
			return ProtoTools.Serialize (msg, valOffset);
		}

		protected override bool TryDeserialize (byte[] qMsg, int offset, out object msg)
		{
			return ProtoTools.TryDeserialize (qMsg, offset, out msg);
		}

	}
}

