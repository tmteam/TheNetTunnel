using System;
using System.Threading.Tasks;

namespace TNT.Presentation.ReceiveDispatching;

public class TaskDispatcher: IDispatcher
{
    private bool _isReleased = false;
    public void Set(RequestMessage message)
    {
        if(_isReleased)
            return;
        Task.Run(() => OnNewMessage?.Invoke(this, message));
    }

    public event Action<IDispatcher, RequestMessage> OnNewMessage;
    public void Release()
    {
        _isReleased = true;
    }
}