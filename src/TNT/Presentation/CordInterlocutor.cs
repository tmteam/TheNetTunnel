using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TNT.Cord;
using TNT.Exceptions;
using TNT.Exceptions.ContractImplementation;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Tcp;

namespace TNT.Presentation
{
    public class CordInterlocutor:ICordInterlocutor
    {
        private readonly ICordMessenger _messenger;
        private readonly IDispatcher _receiveDispatcher;

        private readonly ConcurrentDictionary<int, Action<object[]>> _saySubscribtion 
            = new ConcurrentDictionary<int, Action<object[]>>();

        private readonly ConcurrentDictionary<int, Func<object[], object>> _askSubscribtion
           = new ConcurrentDictionary<int, Func<object[],object>>();


        private readonly ConcurrentDictionary<short, AnswerAwaiter > _answerAwaiters = new ConcurrentDictionary<short, AnswerAwaiter>();

        public int lastUsedAskId = 0;

        public CordInterlocutor(ICordMessenger messenger, IDispatcher receiveDispatcher)
        {
            _messenger = messenger;
            _receiveDispatcher = receiveDispatcher;
            _receiveDispatcher.OnNewMessage += _receiveDispatcher_OnNewMessage;
            _messenger.OnRequest += (_, message)=> _receiveDispatcher.Set(message);
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
            
            var awaiter = new AnswerAwaiter((short)cordId,askId);
            _answerAwaiters.TryAdd(askId, awaiter);
            _messenger.Ask((short)cordId, askId, values);
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


        private void _messenger_OnAns(ICordMessenger sender, short cordId, short askId, object answer)
        {
            //use not conveyor. 
            AnswerAwaiter awaiter;
            _answerAwaiters.TryRemove((short) askId, out awaiter);
            //in case of timeoutException awaiter is still in dictionary
            if(awaiter==null)
                throw new RemoteSerializationException(cordId, askId, true,  $"answer {cordId} / {askId} not awaited");
            //in case of timeoutException, do nothing:
            awaiter.SetResult(answer);
        }
    

        private void _messenger_OnException(ICordMessenger arg1, ExceptionMessage message)
        {
            AnswerAwaiter awaiter;
            _answerAwaiters.TryRemove((short)message.AskId, out awaiter);
            //miss information if exception is general
            awaiter?.SetExceptionalResult(message.Exception);
        }

        private void _receiveDispatcher_OnNewMessage(IDispatcher arg1, CordRequestMessage message)
        {
            try
            {
                if (message.AskId.HasValue)
                {
                    Func<object[], object> handler;
                    _askSubscribtion.TryGetValue(message.Id, out handler);
                    if (handler == null)
                        throw new RemoteContractImplementationException(
                            message.Id, message.AskId, false,
                            $"ask {message.Id} not implemented");
                    object answer = null;
                    answer = handler.Invoke(message.Arguments);
                    _messenger.Ans((short)-message.Id, (short)message.AskId.Value, answer);
                }
                else
                {
                    Action<object[]> handler;
                    _saySubscribtion.TryGetValue(message.Id, out handler);
                    handler?.Invoke(message.Arguments);
                }
            }
            catch (Exception e)
            {
                _messenger.HandleCallException(new RemoteUnhandledException(message.Id, message.AskId, e,
                    $"UnhandledException: {e.ToString()}"));
            }
        }
        class AnswerAwaiter
        {
            private readonly short _cordId;
            private readonly short _askId;
            private readonly ManualResetEvent _event;

            private Exception _exceptionalResult;
            private object _returnResult;

            public AnswerAwaiter(short cordId, short askId)
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
                return _returnResult;
            }

            public void SetExceptionalResult(Exception ex)
            {
                _returnResult = null;
                _exceptionalResult = ex;
                _event.Set();
            }
            public void SetResult(object result)
            {
                _returnResult = result;
                _event.Set();
            }

            
        }
    }
    
}
