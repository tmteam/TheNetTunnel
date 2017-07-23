using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TNT.Presentation
{
    public class ConveyorDispatcher : IDispatcher
    {
        private readonly ConcurrentQueue<RequestMessage> _queue;
        private readonly AutoResetEvent _onNewMessage;
        private bool _exitToken = false;

        public ConveyorDispatcher()
        {
            _onNewMessage= new AutoResetEvent(false);
            _queue = new ConcurrentQueue<RequestMessage>();
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
        public void Set(RequestMessage message)
        {
            _queue.Enqueue(message);
            _onNewMessage.Set();
        }

        public event Action<IDispatcher, RequestMessage> OnNewMessage;

        void ConveyorProcedure()
        {
            while (!_exitToken)
            {
                while (true)
                {
                    RequestMessage message;
                    _queue.TryDequeue(out message);
                    if (message == null)
                        break;

                    OnNewMessage?.Invoke(this, message);
                }
                _onNewMessage.WaitOne(4000);
            }
        }

    }
}