using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.ReceiveDispatching;

namespace TNT.Presentation
{
    public class Interlocutor:IInterlocutor
    {
        private readonly IMessenger _messenger;
        private readonly IDispatcher _receiveDispatcher;

        private readonly ConcurrentDictionary<int, Action<object[]>> _saySubscribtion 
            = new ConcurrentDictionary<int, Action<object[]>>();

        private readonly ConcurrentDictionary<int, Func<object[], object>> _askSubscribtion
           = new ConcurrentDictionary<int, Func<object[],object>>();


        private readonly ConcurrentDictionary<short, AnswerAwaiter > _answerAwaiters = new ConcurrentDictionary<short, AnswerAwaiter>();

        public int lastUsedAskId = 0;

        public Interlocutor(IMessenger messenger, IDispatcher receiveDispatcher)
        {
            _messenger = messenger;
            _receiveDispatcher = receiveDispatcher;
            _receiveDispatcher.OnNewMessage += _receiveDispatcher_OnNewMessage;
            _messenger.OnRequest += (_, message)=> _receiveDispatcher.Set(message);
            _messenger.OnAns += _messenger_OnAns;
            _messenger.OnException += _messenger_OnException;
            _messenger.ChannelIsDisconnected += _messenger_ChannelIsDisconnected;

        }

     

        public void Say(int messageId, object[] values)
        {
            _messenger.Say(messageId, values);
        }

        public T Ask<T>(int messageId, object[] values)
        {
            short askId;
            unchecked {
                askId = (short)Interlocked.Increment(ref lastUsedAskId);
            }
            
            var awaiter = new AnswerAwaiter((short)messageId,askId);
            _answerAwaiters.TryAdd(askId, awaiter);
            _messenger.Ask((short)messageId, askId, values);
            var result = awaiter.WaitOrThrow(10000);
            return (T)result;
        }

        public void SaySubscribe(int messageId, Action<object[]> callback)
        {
            if (!_saySubscribtion.TryAdd(messageId, callback))
                throw new InvalidOperationException($"say {messageId} already subscribed");
        }

        public void AskSubscribe<T>(int messageId, Func<object[], T> callback)
        {
            if (!_askSubscribtion.TryAdd(messageId, (args)=> callback(args)))
                throw new InvalidOperationException($"ask {messageId} already subscribed");
        }

        public void Unsubscribe(int messageId)
        {
            Action<object[]> fakeAction;
            _saySubscribtion.TryRemove(messageId, out fakeAction);
            Func<object[], object> fakeFunc;
            _askSubscribtion.TryRemove(messageId, out fakeFunc);
        }


        private void _messenger_OnAns(IMessenger sender, short messageId, short askId, object answer)
        {
            //use not conveyor. 
            AnswerAwaiter awaiter;
            _answerAwaiters.TryRemove((short) askId, out awaiter);
            //in case of timeoutException awaiter is still in dictionary
            if(awaiter==null)
                throw new RemoteSerializationException(messageId, askId, true,  $"answer {messageId} / {askId} not awaited");
            //in case of timeoutException, do nothing:
            awaiter.SetResult(answer);
        }
    

        private void _messenger_OnException(IMessenger arg1, ExceptionMessage message)
        {
            AnswerAwaiter awaiter;
            _answerAwaiters.TryRemove((short)message.AskId, out awaiter);
            //miss information if exception is general
            awaiter?.SetExceptionalResult(message.Exception);
        }

        private void _receiveDispatcher_OnNewMessage(IDispatcher arg1, RequestMessage message)
        {
            try
            {
                if (message.AskId.HasValue)
                {
                    Func<object[], object> handler;
                    _askSubscribtion.TryGetValue(message.TypeId, out handler);
                    if (handler == null)
                        throw new RemoteContractImplementationException(
                            message.TypeId, message.AskId, false,
                            $"ask {message.TypeId} not implemented");
                    object answer = null;
                    answer = handler.Invoke(message.Arguments);
                    _messenger.Ans((short)-message.TypeId, (short)message.AskId.Value, answer);
                }
                else
                {
                    Action<object[]> handler;
                    _saySubscribtion.TryGetValue(message.TypeId, out handler);
                    handler?.Invoke(message.Arguments);
                }
            }
            catch (Exception e)
            {
                _messenger.HandleCallException(new RemoteUnhandledException(message.TypeId, message.AskId, e,
                    $"UnhandledException: {e.ToString()}"));
            }
        }

        private void _messenger_ChannelIsDisconnected(IMessenger obj)
        {
            _receiveDispatcher.Release();
            while (!_answerAwaiters.IsEmpty)
            {
                AnswerAwaiter awaiter;
                _answerAwaiters.TryRemove(_answerAwaiters.ToArray().First().Key, out awaiter);
                if(awaiter==null)
                    return;
                awaiter.SetExceptionalResult(new ConnectionIsLostException("Connection is lost during the transaction"));
            }
        }
        class AnswerAwaiter
        {
            private readonly short _messageId;
            private readonly short _askId;
            private readonly ManualResetEvent _event;

            private Exception _exceptionalResult;
            private object _returnResult;

            public AnswerAwaiter(short messageId, short askId)
            {
                _messageId = messageId;
                _askId = askId;
                _event = new ManualResetEvent(false);
            }

            public object WaitOrThrow(int msec)
            {
                if (!_event.WaitOne(msec))
                    throw new CallTimeoutException(_messageId, _askId);
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
