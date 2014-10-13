using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

namespace TheTunnel.Light
{
    /// <summary>
    /// Collect quants and parse it into light messages
    /// </summary>
	public class QuantumReceiver
	{
		static int DefaultHeadSize = Marshal.SizeOf(typeof(QuantumHead));

		byte[] qBuff = new byte[0];

		/// <summary>
		/// Set the specified stream of bytes.
		/// </summary>
		/// <param name="bytesfromstream">Bytesfromstream.</param>
		public void Set(byte[] bytesfromstream)
		{
			//Concat new and "old" arrays
			if (qBuff.Length == 0)
				qBuff = bytesfromstream;
			else {
				var temp = new byte[qBuff.Length + bytesfromstream.Length];
				qBuff.CopyTo (temp, 0);
				bytesfromstream.CopyTo (temp, qBuff.Length);
				qBuff = temp;
			}
		
			int offset = 0;
			while(true)
			{
				if (qBuff.Length < DefaultHeadSize+ offset) {
					if (offset > 0)
						qBuff = saveUndone (qBuff,offset);
					return;
				}

				var head = qBuff.ToStruct<QuantumHead> (offset, DefaultHeadSize);

				if (offset + head.length == qBuff.Length) {
					//fullquant
					this.handle (head, qBuff, offset);
					qBuff = new byte[0];
					break;
				} else if (offset + head.length < qBuff.Length) {
					//has additional Lenght
					this.handle (head, qBuff, offset);
					offset += head.length;
				} else {
					qBuff = saveUndone (qBuff,offset);
					break;
				}
			}
		}

		public event Action<QuantumReceiver, QuantumHead,MemoryStream> OnLightMessage;

		public event Action<QuantumReceiver, QuantumHead, byte[]> OnCollectingError;

		byte[] saveUndone(byte[] arr, int offset)
		{
			if (offset == 0)
				return arr;

			byte[] res = new byte[arr.Length - offset];
			Array.Copy (arr, offset, res, 0, res.Length);
			return res;
		}

		void handle(QuantumHead head, byte[] msgFromStream, int quantBeginOffset){

			LightCollector c = null;
			if (collectors.ContainsKey (head.msgId))
				c = collectors [head.msgId];
			else {
				c = new LightCollector ();
				collectors.Add (head.msgId, c);
			}

			if (c.Collect (head, msgFromStream, quantBeginOffset)) {
				// we have got a new light message!
				var stream = c.GetLightMessageStream ();

				collectors.Remove (head.msgId);

				if (stream != null) {
					stream.Position = 0;
					if (OnLightMessage != null)
						OnLightMessage (this, head, stream);
				} else {
					//Oops. An Error has occured during message collecting. 
					if (OnCollectingError != null) {
						byte[] badArray = new byte[msgFromStream.Length - quantBeginOffset];
						Array.Copy (msgFromStream, quantBeginOffset, badArray, 0, badArray.Length);
						OnCollectingError (this, head, badArray);
					}
				}
			}
		}

		Dictionary<int, LightCollector> collectors = new Dictionary<int, LightCollector>();
	}
}

