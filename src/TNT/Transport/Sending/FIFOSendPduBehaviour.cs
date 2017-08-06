using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TNT.Transport.Sending
{
    /// <summary>
    /// Separate Light messages into a quantum sequences
    /// </summary>
    public class FIFOSendPduBehaviour : ISendPduBehaviour
    {
        private const int MaxQuantumSize = 100*1024*1024;
        private readonly ConcurrentQueue<PacketSeparator> _messageQueue = new ConcurrentQueue<PacketSeparator>();
        private int _lastUsedId;

        /// <summary>
        /// Add a message for sending
        /// </summary>
        /// <param name="lightMessage"></param>
        public void Enqueue(MemoryStream lightMessage)
        {
            var id = Interlocked.Increment(ref _lastUsedId);
            var separator = new PacketSeparator(lightMessage, id, MaxQuantumSize);
            _messageQueue.Enqueue(separator);
        }

        public IEnumerable<byte[]> TryDequeue()
        {
            while (true)
            {
                PacketSeparator separator = null;
                if (!_messageQueue.TryDequeue(out separator))
                    yield break;

                byte[] pdu = null;
                while (separator.TryNext(out pdu))
                {
                    yield return pdu;
                }
            }
          
        }
    }
}
