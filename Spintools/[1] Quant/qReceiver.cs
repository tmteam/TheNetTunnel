using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TheTunnel
{
	/// <summary>
	/// Provides functionality to parse input byte stream of quants into messages
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

		byte[] undoneQuant = null;

		/// <summary>
		/// Set the specified stream of bytes.
		/// </summary>
		/// <param name="bytesfromstream">Bytesfromstream.</param>
		public void Set(byte[] bytesfromstream)
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
			while(true)
			{
				var bodyOffset = offset + qheadSize;

				var head = arr.ToStruct<qHead> (0, qheadSize);
				if (head.lenght < qheadSize) {
					undoneQuant = null;
					SendOnError (head, qReceiveError.IncorrectLenght);
					break;
				}
				if (offset + head.lenght == arr.Length) {
					//fullquant
					this.handleQuant (head, arr, bodyOffset);
					break;
				} else if (offset + head.lenght < arr.Length) {
					//has additional Lenght
					this.handleQuant (head, arr, bodyOffset);
					offset += head.lenght;
				} else {
					//save undone Quant
					undoneQuant = new byte[arr.Length - offset];
					Array.Copy (arr, offset, undoneQuant, 0, undoneQuant.Length);
					break;
				}
			}
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
