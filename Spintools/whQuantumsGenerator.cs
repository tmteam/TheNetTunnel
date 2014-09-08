using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WhAlpaTest
{
    public class whQuantumsGenerator 
    {
        public byte[][] Translate(int cord, byte[] msg, ushort maxPackSize, int msgId)
        {
            int headSize = Marshal.SizeOf(typeof(whStartQuantumHead));

            int totalPacks = (int)Math.Ceiling((msg.Length+4) / (double)(maxPackSize + 4 - headSize));
            
            byte[][] ans = new byte[totalPacks][];
            /*
             * ==============
             *  Start quantum
             * ==============
             * [2b] leght
             * [4b] rope
             * [4b] msgId
             * [1b] type (0 = start quantum)
             * [4b] MsgDataLenght
             * [n] Data
             * total: 17+n
             */
            int startQuantDataSize = Math.Min(maxPackSize - headSize, msg.Length);
            
            var startQ = new whStartQuantumHead
            {
                 lenght = (ushort)(headSize+ startQuantDataSize),
                 type = whPacketType.Start,
                 dataLenght = (uint)msg.Length,
                 msgId = msgId,
                 cord = cord,
            };

           
            byte[] bStartQ = new byte[headSize + startQuantDataSize];

            startQ.SetToArray(bStartQ, 0, headSize);
            
            Array.Copy(msg,0, bStartQ, headSize, startQuantDataSize);

            ans[0] = bStartQ;

            int dataOffset = startQuantDataSize;
            /*
             * ==============
             *  Data Quantum
             * ==============
             * [2b] lenght
             * [4b] rope
             * [4b] msgId
             * [1b] type (1 = data quantum)
             * [n] data
             * total: 11+n
             */

            for(int i = 1; dataOffset < msg.Length; i++)
            {
                var dataSize = Math.Min(msg.Length - dataOffset, maxPackSize - 11);
                byte[] datapack = new byte[11 + dataSize];
                Array.Copy(ans[0], datapack, 10);
                Array.Copy(BitConverter.GetBytes((UInt16)datapack.Length), datapack, 2);
                datapack[10] = (byte)whPacketType.Data;
                Array.Copy(msg, dataOffset, datapack, 11, dataSize);
                ans[i] = datapack;
                dataOffset += dataSize;
            }
            return ans;
        }
    }
    [StructLayout( LayoutKind.Explicit, Size = 15)]
    public struct whStartQuantumHead
    {
        [FieldOffset(0)]
        public UInt16 lenght;
        [FieldOffset(2)]
        public Int32 cord;
        [FieldOffset(6)]
        public Int32 msgId;
        [FieldOffset(10)]
        public whPacketType type;
        [FieldOffset(11)]
        public UInt32 dataLenght;
    }
    [StructLayout(LayoutKind.Explicit, Size = 11)]
    public struct whQuantumHead
    {
        [FieldOffset(0)]
        public UInt16 lenght;
        [FieldOffset(2)]
        public Int32 cord;
        [FieldOffset(6)]
        public Int32 msgId;
        [FieldOffset(10)]
        public whPacketType type;
    }
    public enum whPacketType: byte
    {
        Abort = 0,
        Start = 1,
        Data = 2,
    }

}
