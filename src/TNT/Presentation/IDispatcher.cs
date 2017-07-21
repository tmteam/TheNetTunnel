using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using TNT.Cord;

namespace TNT.Presentation
{
    public interface IDispatcher
    {
        void Set(CordRequestMessage message);
        event Action<IDispatcher, CordRequestMessage> OnNewMessage;
        void Release();
    }

    public class NotThreadDispatcher: IDispatcher
    {
        public void Set(CordRequestMessage message)
        {
            OnNewMessage?.Invoke(this, message);
        }

        public event Action<IDispatcher, CordRequestMessage> OnNewMessage;
        public void Release()
        {
            
        }
    }

    public class ConveyorDispatcher : IDispatcher
    {
        private ConcurrentQueue<CordRequestMessage> _queue;
        private AutoResetEvent _onNewMessage;
        private bool _exitToken = false;

        public ConveyorDispatcher()
        {
            _onNewMessage= new AutoResetEvent(false);
            _queue = new ConcurrentQueue<CordRequestMessage>();
            new Thread(ConveyorProcedure)
            {
                IsBackground = true,
                Name = "Conveyor Dispatcher procedure"
            }.Start();
        }

        public void Release()
        {
            _exitToken = true;
        }
        public void Set(CordRequestMessage message)
        {
            _queue.Enqueue(message);
            _onNewMessage.Set();
        }

        private Action<IDispatcher, CordRequestMessage> _onNewMessageDelegate;
        public event Action<IDispatcher, CordRequestMessage> OnNewMessage
        {
            add
            {
                _onNewMessageDelegate += value;
            }
            remove { _onNewMessageDelegate -= value; }
        }

        void ConveyorProcedure()
        {
            while (!_exitToken)
            {
                while (true)
                {
                    CordRequestMessage message;
                    _queue.TryDequeue(out message);
                    if (message == null)
                        break;

                    _onNewMessageDelegate?.Invoke(this, message);
                }
                _onNewMessage.WaitOne(4000);
            }
        }

    }
}