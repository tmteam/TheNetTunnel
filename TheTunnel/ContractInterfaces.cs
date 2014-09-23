using System;

namespace TheTunnel
{
	public interface IDisconnectListener
	{	void OnDisconnect(DisconnectReason reason);
	}

	public interface IDisconnectable
	{	event Action<IDisconnectable> DisconnectMe;
	}
}

