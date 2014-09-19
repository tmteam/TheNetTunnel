using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TheTunnel
{
    public class qSeparator 
    {
		int headSize = Marshal.SizeOf(typeof(qHead));
        
		public byte[][] Separate(byte[] msg, ushort maxQuantSize, int msgId)
        {
            int totalPacks = (int)Math.Ceiling(msg.Length / (double)(maxQuantSize - headSize));
            
            byte[][] ans = new byte[totalPacks][];

            /*
             * ==============
             *  quantum head
             * ==============
             * [2b] leght with head
             * [4b] msgId
             * [1b] type (0 = start quantum)
             * [4b] typeArg
             * [n] body
             * total: 17+n
             */
            
			int dataOffset = 0;

			for (int i = 0; i < totalPacks; i++) {

				ushort qDataSize = (ushort)Math.Min(msg.Length - dataOffset, maxQuantSize - headSize); ; 
				int qArg;
				qType qType = qType.Data;

				if (i == 0) {
					qArg = msg.Length;
					qType = qType.Start;
				} else
					qArg = i;

				var head = new qHead
				{
					lenght = (ushort)(headSize+qDataSize),
					msgId = msgId,
					type = qType,
					typeArg  = qArg 
				};

				byte[] quant = new byte[head.lenght];
				head.SetToArray (quant, 0, headSize);
				Array.Copy (msg, dataOffset, quant, headSize, qDataSize);
				ans [i] = quant;
				dataOffset += qDataSize;
			}
            return ans;
        }
    }


    public enum qType: byte
    {
        Abort = 0,
        Start = 1,
        Data = 2,
    }

}
