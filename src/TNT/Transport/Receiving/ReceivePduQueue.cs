using System;
using System.Collections.Generic;
using System.IO;

namespace TNT.Transport.Receiving
{
    public class ReceivePduQueue
    {
        readonly Queue<MemoryStream> _queue = new Queue<MemoryStream>();

        readonly Dictionary<int, PacketCollector> collectors = new Dictionary<int, PacketCollector>();

        byte[] qBuff = new byte[0];

       

        public void Enqueue(byte[] data)
        {
            //Concat new and "old" arrays
            if (qBuff.Length == 0)
                qBuff = data;
            else
            {
                var temp = new byte[qBuff.Length + data.Length];
                qBuff.CopyTo(temp, 0);
                data.CopyTo(temp, qBuff.Length);
                qBuff = temp;
            }

            int offset = 0;
            while (true)
            {
                if (qBuff.Length <  PduHead.DefaultHeadSize + offset)
                {
                    if (offset > 0)
                        qBuff = saveUndone(qBuff, offset);
                    return;
                }

                var head = qBuff.ToStruct<PduHead>(offset, PduHead.DefaultHeadSize);

                if (offset + head.length == qBuff.Length)
                {
                    //fullquant
                    this.handle(head, qBuff, offset);
                    qBuff = new byte[0];
                    break;
                }
                else if (offset + head.length < qBuff.Length)
                {
                    //has additional Lenght
                    this.handle(head, qBuff, offset);
                    offset += head.length;
                }
                else
                {
                    qBuff = saveUndone(qBuff, offset);
                    break;
                }
            }
        }
        
        public bool IsEmpty { get { return _queue.Count == 0; } }

        public MemoryStream DequeueOrNull()
        {
            if(_queue.Count>0)
                return _queue.Dequeue();
            return null;
        }

        private  byte[] saveUndone(byte[] arr, int offset)
        {
            if (offset == 0)
                return arr;

            byte[] res = new byte[arr.Length - offset];
            Array.Copy(arr, offset, res, 0, res.Length);
            return res;
        }

        private void handle(PduHead head, byte[] msgFromStream, int quantBeginOffset)
        {

            PacketCollector c = null;
            if (collectors.ContainsKey(head.msgId))
                c = collectors[head.msgId];
            else
            {
                c = new PacketCollector();
                collectors.Add(head.msgId, c);
            }

            if (c.Collect(msgFromStream, quantBeginOffset))
            {
                // we have got a new light message!
                var stream = c.GetPduMessageStream();

                collectors.Remove(head.msgId);

                if (stream != null)
                {
                    stream.Position = 0;
                    _queue.Enqueue(stream);
                    //if (OnLightMessage != null)
                    //    OnLightMessage(this, head, stream);
                }
                else
                {
                    //Error. What should we do?
                    //Oops. An Error has occured during message collecting. 
                    /*if (OnCollectingError != null)
                    {
                        byte[] badArray = new byte[msgFromStream.Length - quantBeginOffset];
                        Array.Copy(msgFromStream, quantBeginOffset, badArray, 0, badArray.Length);
                        OnCollectingError(this, head, badArray);
                    }*/
                }
            }
        }

    }
}
