using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WhAlpaTest
{
    public class whReceiver
    {
        public whReceiver()
        {
            qheadSize = Marshal.SizeOf(typeof(whQuantumHead));
            queue = new Dictionary<int, whMsg>();
            MinOutdateIntervalMs = 1000;
        }
        public int MinOutdateIntervalMs { get; set; }

        public void CheckOutDated()
        {
            List<int> odids = new List<int>();
            DateTime dtn = DateTime.Now;
            foreach (var q in queue)
            {
                if ((dtn - q.Value.LastTS).TotalMilliseconds > MinOutdateIntervalMs)
                    odids.Add(q.Key);
            }
            odids.ForEach(id => queue.Remove(id));
        }

        public bool Set(byte[] values)
        {
            int dataLen = values.Length - qheadSize;
            if (dataLen < 0)
                return false;

            var head = values.ToStruct<whQuantumHead>(0, qheadSize);

            if (head.lenght != values.Length) 
                return false;

            int chanelId = head.msgId;
            whMsg msg = null;

            if (queue.ContainsKey(chanelId))
                msg = queue[chanelId];

            if (head.type == whPacketType.Abort)
            {//Aborting msg handling
                if (msg != null)
                    queue.Remove(chanelId);
                return true;
            }
            else if (head.type == whPacketType.Start)
            {//Start msg handling
                if (dataLen >= 4)
                {
                    bool hasMsg = msg != null;

                    msg = new whMsg(head.msgId, head.cord);
                    var AwaitMsgLen = BitConverter.ToUInt32(values, qheadSize);

                    //If the startQuant is too long we should abort it
                    if ((dataLen - 4) > AwaitMsgLen)
                        return false;
                    // if there is already msg with the same id - we should remove it
                    if (hasMsg)
                        queue.Remove(chanelId);

                    msg.Arr = new byte[AwaitMsgLen];
                    msg.bytesDone = dataLen - 4;

                    Array.Copy(values, qheadSize + 4, msg.Arr, 0, dataLen-4);

                    /// if startQuant has full msg's data we will handle it right here
                    if (dataLen - 4 == AwaitMsgLen)
                        SendOnMsg(msg);
                    else//in other case - we should set in in awaiting queue
                        queue.Add(chanelId, msg);
                }
                else
                    return false;
            }
            else if (head.type == whPacketType.Data)
            {
                if (msg == null)//ignore unknown messages
                    return false;

                if (msg.bytesDone + dataLen > msg.Arr.Length)
                {//if we've got too mutch data, we should abort it
                    queue.Remove(chanelId);
                    return false;
                }

                Array.Copy(values, qheadSize, msg.Arr, msg.bytesDone, dataLen);
                msg.bytesDone += dataLen;

                if (msg.bytesDone == msg.Arr.Length)
                {//If we have all neccessary data - it's time to handle it
                    queue.Remove(chanelId);
                    SendOnMsg(msg);
                }
                else
                    msg.LastTS = DateTime.Now;
            }
            else 
                return false;
            return true;
        }
        
        public event Action<whReceiver, whMsg> OnMsg;

        protected void SendOnMsg(whMsg msg)
        {
            if (OnMsg != null)
                OnMsg(this, msg);
        }
        
        int qheadSize;
        Dictionary<int, whMsg> queue;
    }

    public class whMsg
    {
        public whMsg(int id, int cord)
        {
            this.id = id;
            this.cord = cord;
        }
        public DateTime LastTS;
        public int bytesDone;
        public byte[] Arr;
        public readonly int id;
        public readonly int cord;
    }
}
