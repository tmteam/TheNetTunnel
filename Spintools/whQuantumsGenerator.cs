using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WhAlpaTest
{
    public class whQuantumsGenerator 
    {

		int headSize = Marshal.SizeOf(typeof(whQuantHead));
        
		public byte[][] Translate(byte[] msg, ushort maxPackSize, int msgId)
        {
            int totalPacks = (int)Math.Ceiling(msg.Length / (double)(maxPackSize - headSize));
            
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

				ushort qDataSize = (ushort)Math.Min(msg.Length - dataOffset, maxPackSize - headSize); ; 
				int qArg;
				whPacketType qType = whPacketType.Data;

				if (i == 0) {
					qArg = msg.Length;
					qType = whPacketType.Start;
				} else
					qArg = i;

				var head = new whQuantHead
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
    [StructLayout(LayoutKind.Sequential)]
    public struct whQuantHead
    {
        public UInt16 lenght;
        public Int32 msgId;
        public whPacketType type;
        public Int32 typeArg;
    }

    public enum whPacketType: byte
    {
        Abort = 0,
        Start = 1,
        Data = 2,
    }

}
