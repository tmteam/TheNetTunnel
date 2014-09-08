using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WhAlpaTest
{
    public class whSender
    {
        public whSender()
        {
            generator = new whQuantumsGenerator();
            queue = new whSendQueue();
        }
        ushort maxQuantumSize = 1024;
        public ushort MaxQuantumSize
        {
            get{ return maxQuantumSize;}
            set
            {
                if (value <12 )
                    throw new ArgumentException("Packet size cannot be smaller than 12 b");
                if (value > UInt16.MaxValue-1)
                    throw new ArgumentException("Packet size cannot be larger than 65535 b");
                maxQuantumSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cord"></param>
        /// <param name="msg"></param>
        /// <returns>channel id</returns>
        public int Send(int cord, byte[] msg)
        {
            
            var quantums = generator.Translate(cord, msg, maxQuantumSize, Id);
            lock (queue)
            {
                queue.Enqueue(quantums, Id);
            }
            var I = Id;
            Interlocked.Increment(ref Id);
            return I;
        }

        public bool Next(out byte[] quantum, out int channelId)
        {
            lock (queue)
            {
                return queue.Dequeue(out channelId, out quantum);
            }
        }

        public int Lenght
        {
            get { lock (queue) { return queue.Lenght; } }
        }

        public void Clear()
        {
            lock (queue)
            {
                Id = 0;
                queue.Clear();
            }
        }

        int Id = 0;
        whSendQueue queue;
        whQuantumsGenerator generator;
    }
}
