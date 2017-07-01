using System;

namespace TNT
{
	public interface IDisconnectListener
	{	void OnDisconnect(DisconnectReason reason);
	}

	public interface IDisconnectable
	{	event Action<IDisconnectable> DisconnectMe;
	}
}

