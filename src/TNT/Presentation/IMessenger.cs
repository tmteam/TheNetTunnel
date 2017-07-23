using System;
using TNT.Exceptions.Remote;

namespace TNT.Presentation
{
    public interface IMessenger
    {
        void Ask(short id, short askId, object[] arguments);
        void Say(int id, object[] values);
        void Ans(short id, short askId, object value);

        void HandleCallException(RemoteExceptionBase rcException);

        event Action<IMessenger, RequestMessage> OnRequest;

        event Action<IMessenger, short, short, object> OnAns;

        event Action<IMessenger, ExceptionMessage> OnException;

        event Action<IMessenger> ChannelIsDisconnected;
    }
}
