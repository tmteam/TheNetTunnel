using System;

namespace TNT.Presentation
{
    public interface IInterlocutor
    {
        void Say(int messageId, object[] values);
        T Ask<T>(int messageId, object[] values);
        void SaySubscribe(int messageId, Action<object[]> callback);
        void AskSubscribe<T>(int messageId, Func<object[], T> callback);
        void Unsubscribe(int messageId);
    }
}