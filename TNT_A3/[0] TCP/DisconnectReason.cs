using System;

namespace TheTunnel
{
	public enum DisconnectReason:byte
	{
		ContractWish = 0,
		UserWish = 1,
		ConnectionIsLost = 2,
	}
}

