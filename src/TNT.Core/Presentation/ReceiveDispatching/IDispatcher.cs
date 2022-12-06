using System;

namespace TNT.Presentation.ReceiveDispatching;

public interface IDispatcher
{
    void Set(RequestMessage message);
    event Action<IDispatcher, RequestMessage> OnNewMessage;
    void Release();
}