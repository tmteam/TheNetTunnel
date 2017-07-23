using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace TNT.Transport.Sending
{
    /// <summary>
    /// Separate Light messages into a quantum sequences
    /// </summary>
    public class MixedSendMessageSequenceBehaviour : ISendMessageSequenceBehaviour
    {
        private const int maxQuantumSize = 1000;
        private int       _lastUsedId;

        private readonly ConcurrentQueue<MessageSeparator> _messageQueue = new ConcurrentQueue<MessageSeparator>();
        
        /// <summary>
        /// Add a message for sending
        /// </summary>
        /// <param name="lightMessage"></param>
        public void Enqueue(MemoryStream lightMessage)
        {
            var id = Interlocked.Increment(ref _lastUsedId);
            var separator = new MessageSeparator(lightMessage, id, maxQuantumSize);
            _messageQueue.Enqueue(separator);
        }

        /// <summary>
        /// Generate next quantum
        /// </summary>
        /// <param name="quantum"></param>
        /// <param name="messageId"></param>
        /// <returns>true if quantum generated.</returns>
        public bool TryDequeue(out byte[] quantum, out int messageId)
        {
            ConcurrentBag<int> q = new ConcurrentBag<int>();

            MessageSeparator separator = null;
            if (!_messageQueue.IsEmpty && _messageQueue.TryDequeue(out separator))
            {
                separator.TryNext(out quantum);
                messageId = separator.MessageId;
                if(separator.DataLeft > 0)
                    _messageQueue.Enqueue(separator);
                return true;
            }

            quantum = null;
            messageId = 0;
            return false;
        }
    }
}
