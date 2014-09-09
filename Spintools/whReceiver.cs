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
            qheadSize = Marshal.SizeOf(typeof(whQuantHead));
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
                if ((dtn - q.Value.latstTS).TotalMilliseconds > MinOutdateIntervalMs)
                    odids.Add(q.Key);
            }
            odids.ForEach(id => queue.Remove(id));
        }

		byte[] undoneQuant = null;

		public bool Set(byte[] bytesfromstream)
		{
			byte[] arr = null;
			if (undoneQuant == null)
				arr = bytesfromstream;
			else {
				arr = new byte[bytesfromstream.Length + undoneQuant.Length];
				undoneQuant.CopyTo (arr, 0);
				bytesfromstream.CopyTo (arr, undoneQuant.Length);
				undoneQuant = null;
			}

			int offset = 0;
			bool lastIsGood = false;

			while(true)
			{
				var bodyOffset = offset + qheadSize;
				if(bodyOffset<arr.Length)
				{
					var head = arr.ToStruct<whQuantHead> (0, qheadSize);

					if (offset + head.lenght == arr.Length) {
						//fullquant
						return this.HandleQuant (head, arr, bodyOffset);
					} else if (offset + head.lenght < arr.Length) {
						//has additional Lenght
						lastIsGood = this.HandleQuant (head, arr, bodyOffset);
						offset += head.lenght;
					} else {
						//save undone Quant
						undoneQuant = new byte[arr.Length - offset];
						Array.Copy (arr, offset, undoneQuant, 0, undoneQuant.Length);
						return lastIsGood;
					}
				
				}
			}
		}


		bool HandleQuant(whQuantHead head, byte[] stream, int bodyOffset)
        {
			int id = head.msgId;
            whMsg msg = null;


            if (queue.ContainsKey(id))
                msg = queue[id];

            if (head.type == whPacketType.Abort)
            {//Aborting msg handling
                if (msg != null)
                    queue.Remove(id);
                return true;
            }
            else if (head.type == whPacketType.Start)
            {//Start msg handling
            	bool hasMsg = msg != null;

                msg = new whMsg(head.msgId);
				var AwaitMsgLen = head.typeArg;

                // if there is already msg with the same id - we should remove it
                if (hasMsg)
                    queue.Remove(id);

                msg.body = new byte[AwaitMsgLen];


				int bodyLenght = head.lenght - qheadSize;

				msg.bytesDone = bodyLenght;

				Array.Copy (stream,bodyOffset, msg.body,0, bodyLenght);

				/// if startQuant has full msg's data we will handle it right here
				if (bodyLenght == AwaitMsgLen)
                    SendOnMsg(msg);
                else//in other case - we should set in in awaiting queue
                    queue.Add(id, msg);
            }
            else if (head.type == whPacketType.Data)
            {
                if (msg == null)//ignore unknown messages
                    return false;

				int bodyLenght = head.lenght - qheadSize;

				if (msg.bytesDone + bodyLenght > msg.body.Length)
                {//if we've got too mutch data, we should abort it
                    queue.Remove(id);
                    return false;
                }

				Array.Copy(stream, bodyOffset, msg.body, msg.bytesDone, bodyLenght);
				msg.bytesDone += bodyLenght;

                if (msg.bytesDone == msg.body.Length)
                {//If we have all neccessary data - it's time to handle it
                    queue.Remove(id);
                    SendOnMsg(msg);
                }
                else
                    msg.latstTS = DateTime.Now;
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
        public whMsg(int id)
        {
            this.id = id;
        }
        public DateTime latstTS;
        public int bytesDone;
        public byte[] body;
        public readonly int id;

    }
}
