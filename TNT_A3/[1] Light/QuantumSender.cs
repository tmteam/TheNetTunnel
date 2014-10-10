using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace TheTunnel.Light
{
	public class QuantumSender
	{
		public void Set(MemoryStream lightMessage)
		{
			var id = Interlocked.Increment (ref msgId);
			LightSeparator newSep = null;
			if (used.Count > 0) {
				newSep = used [0];
				used.RemoveAt (0);
			}
			else
				newSep	= new LightSeparator ();
			newSep.Initialize (lightMessage, id);
			queue.Add (newSep);

		}

		public bool TryNext( int maxQuantumSize, out byte[] quantum, out int msgId)
		{

			if (queue.Count == 0) {
				msgId = 0;
				quantum = null;
				return false;
			}
			if (queue.Count >= qPos)
				qPos = 0;

			var q = queue [qPos];
			quantum = q.Next (maxQuantumSize);

			msgId = q.MsgId;

			if (q.DataLeft <= 0) {
				used.Add (q);
				queue.RemoveAt (qPos);
			}
			else
				qPos++;

			return true;
		}

		public void Clear()	{
			queue.Clear ();
		}

		int msgId;
		int qPos = 0;
		List<LightSeparator> queue = new List<LightSeparator>();
		List<LightSeparator> used = new List<LightSeparator>();
	
	}
}

