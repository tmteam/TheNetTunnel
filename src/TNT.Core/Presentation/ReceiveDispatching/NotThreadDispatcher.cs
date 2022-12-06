using System;

namespace TNT.Presentation.ReceiveDispatching;

public class NotThreadDispatcher: IDispatcher
{
    public event Action<IDispatcher, RequestMessage> OnNewMessage;

    public void Set(RequestMessage message)
    {
        OnNewMessage?.Invoke(this, message);
    }
    
    public void Release()
    {
            
    }
}