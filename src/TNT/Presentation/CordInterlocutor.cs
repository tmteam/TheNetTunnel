using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNT.Cord;

namespace TNT.Presentation
{
    public class CordInterlocutor:ICordInterlocutor
    {
        private readonly ICordMessenger _messenger;
        private readonly ConcurrentDictionary<int, Action<object[]>> _saySubscribtion 
            = new ConcurrentDictionary<int, Action<object[]>>();

        private readonly ConcurrentDictionary<int, Func<object[], object>> _askSubscribtion
           = new ConcurrentDictionary<int, Func<object[],object>>();


        private readonly ConcurrentDictionary<short, AnswerAwaiter > _answerAwaiters = new ConcurrentDictionary<short, AnswerAwaiter>();

        public int lastUsedAskId = 0;

        public CordInterlocutor(ICordMessenger messenger)
        {
            _messenger = messenger;
            _messenger.OnSay += _messenger_OnSay;
            _messenger.OnAsk += _messenger_OnAsk;
            _messenger.OnAns += _messenger_OnAns;
        }

        public void Say(int cordId, object[] values)
        {
            _messenger.Say(cordId, values);
        }

        public T Ask<T>(int cordId, object[] values)
        {
            short askId;
            unchecked {
                askId = (short)Interlocked.Increment(ref lastUsedAskId);
            }
            var awaiter = new AnswerAwaiter();
            _answerAwaiters.TryAdd(askId, awaiter);
            _messenger.Ask((short)cordId, (short)askId, values);
            awaiter.Event.WaitOne();
            return (T) awaiter.ReturnResult;
        }

        public void SaySubscribe(int cordId, Action<object[]> callback)
        {
            if (!_saySubscribtion.TryAdd(cordId, callback))
                throw new InvalidOperationException($"say {cordId} already subscribed");
        }

        public void AskSubscribe<T>(int cordId, Func<object[], T> callback)
        {
            if (!_askSubscribtion.TryAdd(cordId, (args)=> callback(args)))
                throw new InvalidOperationException($"ask {cordId} already subscribed");
        }

        public void Unsubscribe(int cordId)
        {
            Action<object[]> fakeAction;
            _saySubscribtion.TryRemove(cordId, out fakeAction);
            Func<object[], object> fakeFunc;
            _askSubscribtion.TryRemove(cordId, out fakeFunc);
        }


        private void _messenger_OnAns(ICordMessenger sender, int cordId, int askId, object answer)
        {
            AnswerAwaiter awaiter;
            _answerAwaiters.TryRemove((short) askId, out awaiter);
            if(awaiter==null)
                throw new InvalidOperationException($"answer {cordId} / {askId} not awaited");
            awaiter.SetResult(answer);
        }

        private void _messenger_OnAsk(ICordMessenger sender, int cordId, int askId, object[] args)
        {
            Func<object[],object> handler;
            _askSubscribtion.TryGetValue(cordId, out handler);
            if (handler == null)
                throw new InvalidOperationException($"ask {cordId} not implemented");
            var answer = handler.Invoke(args);
            _messenger.Ans((short)cordId, (short)askId, answer);
        }

        private void _messenger_OnSay(ICordMessenger sender, int cordId, object[] args)
        {
            Action<object[]> handler;
            _saySubscribtion.TryGetValue(cordId, out handler);
            handler?.Invoke(args);
        }

        class AnswerAwaiter
        {
            public AnswerAwaiter()
            {
                Event = new ManualResetEvent(false);
            }

            public void SetResult(object result)
            {
                ReturnResult = result;
                Event.Set();
            }
            public ManualResetEvent Event { get;}
            public object ReturnResult { get; private set; }
        }
    }
    
}
