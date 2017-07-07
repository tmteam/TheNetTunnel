using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace TheTunnel.Light
{
    /// <summary>
    /// Separate Light messages into a quantum sequences
    /// </summary>
    public class QuantumSender
    {
        object locker = new object();

        /// <summary>
        /// Add a message for sending
        /// </summary>
        /// <param name="lightMessage"></param>
        public void Set(MemoryStream lightMessage)
        {
            lock (locker)
            {
                var id = Interlocked.Increment(ref msgId);
                LightSeparator newSep = null;
                if (used.Count > 0)
                {
                    newSep = used[0];
                    used.RemoveAt(0);
                }
                else
                    newSep = new LightSeparator();
                newSep.Initialize(lightMessage, id);
                queue.Add(newSep);
            }
        }

        /// <summary>
        /// Generate next quantum
        /// </summary>
        /// <param name="maxQuantumSize"></param>
        /// <param name="quantum"></param>
        /// <param name="msgId"></param>
        /// <returns>true if quantum generated.</returns>
        public bool TryNext(int maxQuantumSize, out byte[] quantum, out int msgId)
        {
            lock (locker)
            {
                if (queue.Count == 0)
                {
                    msgId = 0;
                    quantum = null;
                    return false;
                }
                if (queue.Count >= qPos)
                    qPos = 0;

                var q = queue[qPos];
                quantum = q.Next(maxQuantumSize);

                msgId = q.MsgId;

                if (q.DataLeft <= 0)
                {
                    used.Add(q);
                    queue.RemoveAt(qPos);
                }
                else
                    qPos++;
            }
            return true;
        }

        /// <summary>
        /// Clear 
        /// </summary>
        public void Clear()
        {
            queue.Clear();
        }

        int msgId;
        //last taken queue item
        int qPos = 0;
        //Separation queue
        List<LightSeparator> queue = new List<LightSeparator>();
        // unused separators
        List<LightSeparator> used = new List<LightSeparator>();

    }
}

