using System;
using TNT.Exceptions;
using TNT.Exceptions.Remote;

namespace TNT.Cord
{
    public interface ICordMessenger
    {
        void Ask(short id, short askId, object[] arguments);
        void Say(int id, object[] values);
        void Ans(short id, short askId, object value);

        void HandleCallException(RemoteExceptionBase rcException);

        event Action<ICordMessenger, CordRequestMessage> OnRequest;

        event Action<ICordMessenger, short, short, object> OnAns;

        event Action<ICordMessenger, ExceptionMessage> OnException;

        event Action<ICordMessenger> ChannelIsDisconnected;
    }
}
