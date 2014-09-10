using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TheTunnel
{
    public class qSender
    {
        public qSender()
        {
            separator = new qSeparator();
            queue = new qSendQueue();
        }
		ushort maxQuantSize = 1024;
        public ushort MaxQuantSize
        {
            get{ return maxQuantSize;}
            set
            {
                if (value <12 )
                    throw new ArgumentException("Quant size cannot be smaller than 12 b");
                if (value > UInt16.MaxValue-1)
                    throw new ArgumentException("Quant size cannot be larger than 65535 b");
                maxQuantSize = value;
            }
        }

        
		public int Send(byte[] msg)
        {
            var quantums = separator.Separate(msg, maxQuantSize, Id);
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
        qSendQueue queue;
		qSeparator separator;
    }
}
