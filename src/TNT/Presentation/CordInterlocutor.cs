using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNT.Cord;
using TNT.Exceptions;

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
            _messenger.OnException += _messenger_OnException;
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
            
            var awaiter = new AnswerAwaiter(cordId,askId);
            _answerAwaiters.TryAdd(askId, awaiter);
            _messenger.Ask((short)cordId, (short)askId, values);
            var result = awaiter.WaitOrThrow(10000);
            return (T)result;
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
                throw new RemoteSideSerializationException(cordId, askId, $"answer {cordId} / {askId} not awaited");
            awaiter.SetResult(answer);
        }

        private void _messenger_OnAsk(ICordMessenger sender, int cordId, int askId, object[] args)
        {
            Func<object[],object> handler;
            _askSubscribtion.TryGetValue(cordId, out handler);
            if (handler == null)
                throw new RemoteContractImplementationException(cordId,
                    $"ask {cordId} not implemented");
            object answer = null;

            try
            {
                answer = handler.Invoke(args);
            }
            catch (Exception e)
            {
                _messenger.HandleCallException(new RemoteSideUnhandledException(cordId, askId,
                    $"UnhandledException: {e.ToString()}"));
            }
            _messenger.Ans((short)-cordId, (short)askId, answer);
        }

        private void _messenger_OnSay(ICordMessenger sender, int cordId, object[] args)
        {
            Action<object[]> handler;
            _saySubscribtion.TryGetValue(cordId, out handler);
            try
            {
                handler?.Invoke(args);
            }
            catch (Exception e)
            {
                _messenger.HandleCallException(new RemoteSideUnhandledException(cordId, null,
                    $"UnhandledException: {e.ToString()}"));
            }
        }
        private void _messenger_OnException(ICordMessenger arg1, ExceptionMessage message)
        {
            AnswerAwaiter awaiter;
            _answerAwaiters.TryRemove((short)message.AskId, out awaiter);

            awaiter?.SetExceptionalResult(message.Exception);
        }
        class AnswerAwaiter
        {
            private readonly int _cordId;
            private readonly int _askId;
            private Exception _exceptionalResult;

            public AnswerAwaiter(int cordId, int askId)
            {
                _cordId = cordId;
                _askId = askId;
                _event = new ManualResetEvent(false);
            }

            public object WaitOrThrow(int msec)
            {
                if (!_event.WaitOne(msec))
                    throw new CallTimeoutException(_cordId, _askId);
                if (_exceptionalResult != null)
                    throw _exceptionalResult;
                return ReturnResult;
            }

            public void SetExceptionalResult(Exception ex)
            {
                ReturnResult = null;
                _exceptionalResult = ex;
                _event.Set();
            }
            public void SetResult(object result)
            {
                ReturnResult = result;
                _event.Set();
            }

            private ManualResetEvent _event;
            public object ReturnResult { get; private set; }
        }
    }
    
}
