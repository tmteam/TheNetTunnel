using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheTunnel
{
    public class qSendQueue
    {
        public void Enqueue(byte[][] quantums, int channelId)
        {
            if (!queue.ContainsKey(channelId)){
                queuePos.Add(channelId, 0);
                queue.Add(channelId, quantums);
                keys.Add(channelId);
            }
            else{
                lenght -= (queue[channelId].Length - queuePos[channelId]);
                queuePos[channelId] = 0;
                queue[channelId] = quantums;
            }
            lenght += quantums.Length;
        }

        public void Clear()
        {
            lenght = 0;
            pos = 0;
            queue.Clear();
            queuePos.Clear();
            keys.Clear();
        }

        public bool Dequeue(out int channelId, out byte[] quantum)
        {
            if (keys.Count == 0)
            {
                channelId = 0;
                quantum = null;
                return false;
            }

            lenght--;

            if (pos >= keys.Count)
                pos = 0;
            
            channelId = keys[pos];
            var offset = queuePos[channelId];
            var msg = queue[channelId];
            quantum = msg[offset];

            if (msg.Length == offset +1)
            {
                queue.Remove(channelId);
                queuePos.Remove(channelId);
                keys.Remove(channelId);
            }
            else
            {
                queuePos[channelId]++;
                pos++;
            }
            return true;
        }

        int lenght = 0;
        public int Lenght
        {
            get { return lenght;}
        }

        Dictionary<int, byte[][]> queue = new Dictionary<int, byte[][]>();
        Dictionary<int, int> queuePos = new Dictionary<int, int>();
        List<int> keys = new List<int>();
        int pos = 0;
    }
}
