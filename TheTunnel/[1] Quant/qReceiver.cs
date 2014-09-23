using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TheTunnel
{
	/// <summary>
	/// Provides functionality to parse input byte stream of quants into mssages
	/// </summary>
    public class qReceiver
    {
        public qReceiver()
        {
            qheadSize = Marshal.SizeOf(typeof(qHead));
            queue = new Dictionary<int, qMsg>();
            MinOutdateIntervalMs = 1000;
			MsgMaxSize = 8000000;
        }
		/// <summary>
		/// Gets or sets maximum size of a message in bytes.
		/// </summary>
		/// <value>The size of the max message.</value>
		public int MsgMaxSize{ get; set; }
		/// <summary>
		/// Gets or sets the minimum quant outdate interval ms.
		/// </summary>
		/// <value>The maximum interval of quant part awaiting [ms].</value>
        public int MinOutdateIntervalMs { get; set; }

		/// <summary>
		/// Checks the out dated undone quants.
		/// </summary>
        public void CheckOutDated()
        {
            List<int> odids = new List<int>();
            DateTime dtn = DateTime.Now;
            foreach (var q in queue)
            {
				if ((q.Value.body.Length> q.Value.bytesDone) 
					&& (dtn - q.Value.latstTS).TotalMilliseconds > MinOutdateIntervalMs)
                    odids.Add(q.Key);
            }
            odids.ForEach(id => queue.Remove(id));
        }
		byte[] qBuff = new byte[0];

		/// <summary>
		/// Set the specified stream of bytes.
		/// </summary>
		/// <param name="bytesfromstream">Bytesfromstream.</param>
		public void Set(byte[] bytesfromstream)
		{
			qBuff = qBuff.Concat (bytesfromstream).ToArray ();
			int offset = 0;
			while(true)
			{
				if (qBuff.Length < qheadSize) {
					if (offset > 0)
					 qBuff = saveUndone (qBuff,offset);
					return;
				}

				var bodyOffset = offset + qheadSize;
				var head = qBuff.ToStruct<qHead> (0, qheadSize);

				if (offset + head.lenght == qBuff.Length) {
					//fullquant
					this.handleQuant (head, qBuff, bodyOffset);
					qBuff = new byte[0];
					break;
				} else if (offset + head.lenght < qBuff.Length) {
					//has additional Lenght
					this.handleQuant (head, qBuff, bodyOffset);
					offset += head.lenght;
				} else {
					qBuff = saveUndone (qBuff,offset);
					break;
				}
			}
		}

		byte[] saveUndone(byte[] arr, int offset)
		{
			byte[] res = new byte[arr.Length - offset];
			Array.Copy (arr, offset, res, 0, res.Length);
			return res;
		}

		bool handleQuant(qHead head, byte[] stream, int bodyOffset)
        {
			int id = head.msgId;
            qMsg msg = null;


            if (queue.ContainsKey(id))
                msg = queue[id];

			if (head.type == qType.Abort) {//Aborting msg handling
				if (msg != null)
					queue.Remove (id);
				return true;
			} else if (head.type == qType.Start) {//Start msg handling
				bool hasMsg = msg != null;

				msg = new qMsg (head.msgId);
				var AwaitMsgLen = head.typeArg;


				// if there is already msg with the same id - we should remove it
				if (hasMsg)
					queue.Remove (id);

				if (AwaitMsgLen < 0 || AwaitMsgLen > MsgMaxSize) {
					SendOnError ( head, qReceiveError.TooLargeMessage);
					return false;
				}
				msg.body = new byte[AwaitMsgLen];

				int bodyLenght = head.lenght - qheadSize;

				msg.bytesDone = bodyLenght;

				Array.Copy (stream, bodyOffset, msg.body, 0, bodyLenght);

				/// if startQuant has full msg's data we will handle it right here
				if (bodyLenght == AwaitMsgLen)
					SendOnMsg (msg);
				else//in other case - we should set in in awaiting queue
                    queue.Add (id, msg);
			} else if (head.type == qType.Data) {
				if (msg == null) {//ignore unknown messages
					SendOnError (head, qReceiveError.UnknownId);
					return false;
				}
				int bodyLenght = head.lenght - qheadSize;

				if (msg.bytesDone + bodyLenght > msg.body.Length) {//if we've got too mutch data, we should abort it
					queue.Remove (id);
					SendOnError (head, qReceiveError.IncorrectLenght);
					return false;
				}

				Array.Copy (stream, bodyOffset, msg.body, msg.bytesDone, bodyLenght);
				msg.bytesDone += bodyLenght;

				if (msg.bytesDone == msg.body.Length) {//If we have all neccessary data - it's time to handle it
					queue.Remove (id);
					SendOnMsg (msg);
				} else
					msg.latstTS = DateTime.Now;
			} else {
				SendOnError (head, qReceiveError.BadHead);
				return false;
			}
			return true;
        }

		public delegate void delQOnError(qReceiver sender, qHead head, qReceiveError err);

		public event delQOnError OnError;

		protected void SendOnError(qHead head,qReceiveError err )
		{
			if(OnError!=null)
				OnError(this, head, err);
		}
        public event Action<qReceiver, qMsg> OnMsg;

        protected void SendOnMsg(qMsg msg)
        {
            if (OnMsg != null)
                OnMsg(this, msg);
        }
        
        int qheadSize;
        Dictionary<int, qMsg> queue;
    }
	public enum qReceiveError
	{
		IncorrectLenght,
		TooLargeMessage,
		UnknownId,
		BadHead,
		IncorrectSequence,

	}
    public class qMsg
    {
        public qMsg(int id)
        {
            this.id = id;
        }
        public DateTime latstTS;
        public int bytesDone;
        public byte[] body;
        public readonly int id;

    }
}
