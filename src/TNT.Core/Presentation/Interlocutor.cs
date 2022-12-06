using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.ReceiveDispatching;

namespace TNT.Presentation;

/// <summary>
/// Implements interaction with remote contract via specified messenger
/// </summary>
public class Interlocutor:IInterlocutor
{
    private readonly IMessenger  _messenger;
    private readonly IDispatcher _receiveDispatcher;
    private readonly int         _maxAnsDelay;

    private readonly ConcurrentDictionary<int, Action<object[]>> _saySubscribtion 
        = new ConcurrentDictionary<int, Action<object[]>>();

    private readonly ConcurrentDictionary<int, Func<object[], object>> _askSubscribtion
        = new ConcurrentDictionary<int, Func<object[],object>>();


    private readonly ConcurrentDictionary<short, AnswerAwaiter > _answerAwaiters = new ConcurrentDictionary<short, AnswerAwaiter>();

    public int lastUsedAskId;

    public Interlocutor(IMessenger messenger, IDispatcher receiveDispatcher, int maxAnsDelay = 3000)
    {
        _messenger = messenger;
        _receiveDispatcher = receiveDispatcher;
        _maxAnsDelay = maxAnsDelay;
        _receiveDispatcher.OnNewMessage += _receiveDispatcher_OnNewMessage;
        _messenger.OnRequest += (_, message)=> _receiveDispatcher.Set(message);
        _messenger.OnAns += _messenger_OnAns;
        _messenger.ChannelIsDisconnected += _messenger_ChannelIsDisconnected;
        _messenger.OnException += _messenger_OnException1;
    }
        
    /// <summary>
    /// Sends "Say" message with "values" arguments
    /// </summary>
    ///<exception cref="LocalSerializationException"></exception>
    ///<exception cref="ConnectionIsLostException"></exception>
    ///<exception cref="ArgumentException">wrong message id</exception>
    public void Say(int messageId, object[] values) {
        _messenger.Say((short)messageId, values);
    }
    /// <summary>
    /// Remote method call. Blocking.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    ///<exception cref="ArgumentException">wrong message id</exception>
    ///<exception cref="TntCallException"></exception>
    ///<exception cref="LocalSerializationException">some of the argument type serializers or deserializers are not implemented, 
    /// or not the same as specified in the contract</exception>
    public T Ask<T>(int messageId, object[] values)
    {
        short askId;
        unchecked {
            askId = (short)Interlocked.Increment(ref lastUsedAskId);
        }
            
        var awaiter = new AnswerAwaiter((short)messageId,askId);
        _answerAwaiters.TryAdd(askId, awaiter);
        _messenger.Ask((short)messageId, askId, values);
        var result = awaiter.WaitOrThrow(_maxAnsDelay);
        return (T)result;
    }

    /// <summary>
    /// Set income Say message Handler
    /// </summary>
    ///<exception cref="ArgumentException">already contains say messageId handler</exception>
    public void SetIncomeSayCallHandler(int messageId, Action<object[]> callback)
    {
        if (!_saySubscribtion.TryAdd(messageId, callback))
        {
            throw new ArgumentException($"say {messageId} already subscribed");
        }
    }
    /// <summary>
    /// Set income Say message Handler
    /// </summary>
    ///<exception cref="ArgumentException">already contains ask messageId handler</exception>
    public void SetIncomeAskCallHandler<T>(int messageId, Func<object[], T> callback)
    {
        if (!_askSubscribtion.TryAdd(messageId, (args) => callback(args)))
        {
            throw new ArgumentException($"ask {messageId} already subscribed");
        }
    }
    /// <summary>
    /// Unsubscribes request handler
    /// </summary>
    public void Unsubscribe(int messageId)
    {
        _saySubscribtion.TryRemove(messageId, out var fakeAction);
        _askSubscribtion.TryRemove(messageId, out var fakeFunc);
    }


    private void _messenger_OnAns(IMessenger sender, short messageId, short askId, object answer)
    {
        //use not conveyor. 
        _answerAwaiters.TryRemove(askId, out var awaiter);
        //in case of timeoutException awaiter is still in dictionary
        if (awaiter == null)
        {
            _messenger.HandleRequestProcessingError(new ErrorMessage(
                messageId, 
                askId, 
                ErrorType.SerializationError, $"answer {messageId} / {askId} not awaited"),false);
            return;
        }
        //in case of timeoutException, do nothing:
        awaiter.SetSuccesfullyResult(answer);
    }

    private void _messenger_OnException1(IMessenger arg1, Exception exception)
    {
        var callException = exception as TntCallException;
        if (callException?.AskId != null)
        {
            _answerAwaiters.TryRemove((short) callException.AskId, out var awaiter);
            awaiter?.SetExceptionalResult(exception);
        }
        else
        {
            // what shall we do?
        }
    }

    private void _receiveDispatcher_OnNewMessage(IDispatcher arg1, RequestMessage message)
    {
        try
        {
            if (message.AskId.HasValue)
            {
                _askSubscribtion.TryGetValue(message.TypeId, out var askHandler);

                if (askHandler == null)
                {
                    _messenger.HandleRequestProcessingError(
                        new ErrorMessage(
                            messageId: message.TypeId,
                            askId: message.AskId,
                            type: ErrorType.ContractSignatureError,
                            additionalExceptionInformation: $"ask {message.TypeId} not implemented"), false);
                    return;
                }
                object answer = null;
                answer = askHandler.Invoke(message.Arguments);
                _messenger.Ans((short) -message.TypeId, message.AskId.Value, answer);
            }
            else
            {
                _saySubscribtion.TryGetValue(message.TypeId, out var sayHandler);
                sayHandler?.Invoke(message.Arguments);
            }
        }
        catch (LocalSerializationException serializationException)
        {
            _messenger.HandleRequestProcessingError(
                new ErrorMessage(
                    message.TypeId,
                    message.AskId,
                    ErrorType.SerializationError,
                    serializationException.ToString()), true);
        }
        catch (Exception e)
        {
            _messenger.HandleRequestProcessingError(
                new ErrorMessage(
                    message.TypeId, 
                    message.AskId, 
                    ErrorType.UnhandledUserExceptionError, 
                    $"UnhandledException: {e.ToString()}"), false);
        }
    }

    private void _messenger_ChannelIsDisconnected(IMessenger obj, ErrorMessage cause)
    {
        _receiveDispatcher.Release();
        while (!_answerAwaiters.IsEmpty)
        {
            _answerAwaiters.TryRemove(_answerAwaiters.ToArray().First().Key, out var awaiter);
            if (awaiter == null)
                return;
            if (cause == null)
                awaiter.SetConnectionIsLostResult();
            else
                awaiter.SetErrorResult(cause);
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

        /// <summary>
        /// Waits for the operation result
        /// </summary>
        ///<exception cref="CallTimeoutException"></exception>
        ///<exception cref="RemoteException"></exception>
        ///<exception cref="ConnectionIsLostException"></exception>
        public object WaitOrThrow(int msec)
        {
            if (!_event.WaitOne(msec))
                throw new CallTimeoutException(_messageId, _askId);
            if (_exceptionalResult != null)
                throw _exceptionalResult;
            return _returnResult;
        }

        public void SetConnectionIsLostResult()
        {
            _returnResult = null;
            _exceptionalResult = new ConnectionIsLostException($"Connection is lost during the transaction. MessageId: {_messageId}, AskId: {_askId}", _messageId, _askId);
            _event.Set();
        }
        public void SetExceptionalResult(Exception ex)
        {
            _returnResult = null;
            _exceptionalResult = ex;
            _event.Set();
        }
        public void SetErrorResult(ErrorMessage error)
        {
            SetExceptionalResult(error.Exception);
        }
            
        public void SetSuccesfullyResult(object result)
        {
            _returnResult = result;
            _event.Set();
        }

            
    }
}