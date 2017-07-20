using System;
using TNT.Exceptions;

namespace TNT.Cord
{
    public interface ICordMessenger
    {
        void Ask(short id, short askId, object[] arguments);
        void Say(int id, object[] values);
        void Ans(short id, short askId, object value);
        event Action<ICordMessenger, int, int, object[]> OnAsk;
        event Action<ICordMessenger, int, int, object> OnAns;
        event Action<ICordMessenger, int, int, ExceptionMessage> OnAnsException;

        event Action<ICordMessenger, int, object[]> OnSay;
    }

    public class ExceptionMessage
    {
        public int CordId { get; set; }
        public int AskId { get; set; }
        public RemoteCallExceptionId ExceptionType { get; set; }
        public string AdditionalExceptionInformation { get; set; }
    }
}
