using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CommonTestTools
{
    public sealed class EventAwaiter<TEventArgs> : INotifyCompletion
    {
        private readonly ConcurrentQueue<TEventArgs> _mEvents = new ConcurrentQueue<TEventArgs>();
        private Action _mContinuation;
        private readonly ManualResetEvent _eventsRaised = new ManualResetEvent(false);

        public TEventArgs WaitOne()
        {
            _eventsRaised.WaitOne();
            return GetResult();
        }
        public TEventArgs WaitOneOrDefault(int msec)
        {
            if (_eventsRaised.WaitOne(msec))
                return GetResult();
            else
                return default(TEventArgs);
        }
        #region Члены, вызываемые конечным автоматом
        // Конечный автомат сначала вызывает этот метод для получения
        // объекта ожидания; возвращаем текущий объект
        public EventAwaiter<TEventArgs> GetAwaiter() { return this; }

        // Сообщает конечному автомату, произошли ли какие-либо события
        public Boolean IsCompleted { get { return _mEvents.Count > 0; } }
        // Конечный автомат сообщает, какой метод должен вызываться позднее;
        // сохраняем полученную информацию
        public void OnCompleted(Action continuation)
        {
            Volatile.Write(ref _mContinuation, continuation);
        }
        // Конечный автомат запрашивает результат, которым является
        // результат оператора await
        public TEventArgs GetResult()
        {
            TEventArgs e;
            _mEvents.TryDequeue(out e);
            return e;
        }
        #endregion


        // Теоретически может вызываться несколькими потоками одновременно,
        // когда каждый поток инициирует событие
        public void EventRaised(object sender, TEventArgs eventArgs)
        {

            _mEvents.Enqueue(eventArgs); // Сохранение EventArgs
                                         // для возвращения из GetResult/await
                                         // Если имеется незавершенное продолжение, поток забирает его

            var continuation = Interlocked.Exchange(ref _mContinuation, null);
            continuation?.Invoke(); // Продолжение выполнение конечного автомата
            _eventsRaised.Set();

        }
    }
}