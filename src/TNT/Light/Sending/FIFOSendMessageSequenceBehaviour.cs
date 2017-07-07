using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace TNT.Light.Sending
{
    /// <summary>
    /// Separate Light messages into a quantum sequences
    /// </summary>
    public class FIFOSendMessageSequenceBehaviour : ISendMessageSequenceBehaviour
    {
        private const int maxQuantumSize = 1000;
        private readonly ConcurrentQueue<MessageSeparator> _messageQueue = new ConcurrentQueue<MessageSeparator>();
        private int _lastUsedId;
        private MessageSeparator undoneMessage = null;

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
            MessageSeparator separator = null;
            if (undoneMessage == null)
                _messageQueue.TryDequeue(out separator);
            else
                separator = undoneMessage;

            if (separator != null)
            {
                if (separator.TryNext(out quantum))
                {
                    messageId = separator.MessageId;
                    undoneMessage = separator.DataLeft <= 0 ? null : separator; 
                    return true;
                }
            }

            undoneMessage = null;
            quantum = null;
            messageId = 0;
            return false;
        }
    }
}
