using System;

namespace TNT.Presentation
{
    public interface ICordInterlocutor
    {

        void Say(int cordId, object[] values);
        T Ask<T>(int cordId, object[] values);
        void SaySubscribe(int cordId, Action<object[]> callback);
        void AskSubscribe<T>(int cordId, Func<object[], T> callback);
        void Unsubscribe(int cordId);
    }
}