using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace TNT.Light
{
    public interface IDispatcher
    {
        void Set(MemoryStream stream);
        event Action<IDispatcher, MemoryStream> OnNewMessage;
        void Release();
    }

    public class NotThreadDispatcher: IDispatcher
    {
        public void Set(MemoryStream stream)
        {
            OnNewMessage?.Invoke(this, stream);
        }

        public event Action<IDispatcher, MemoryStream> OnNewMessage;
        public void Release()
        {
            
        }
    }

    public class ConveyorDispatcher : IDispatcher
    {
        private ConcurrentQueue<MemoryStream> _queue;
        private AutoResetEvent _onNewMessage;
        private bool _exitToken = false;

        public ConveyorDispatcher()
        {
            _onNewMessage= new AutoResetEvent(false);
            _queue = new ConcurrentQueue<MemoryStream>();
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
        public void Set(MemoryStream stream)
        {
            _queue.Enqueue(stream);
            _onNewMessage.Set();
        }

        private Action<IDispatcher, MemoryStream> _onNewMessageDelegate;
        public event Action<IDispatcher, MemoryStream> OnNewMessage
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
                    MemoryStream message;
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