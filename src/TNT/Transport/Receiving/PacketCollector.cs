using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TNT.Transport.Receiving
{
    /// <summary>
    /// Collect single light message from quants
    /// </summary>
    public class PacketCollector
    {
        private static readonly int DefaultHeadSize = Marshal.SizeOf(typeof(PduHead));

        private MemoryStream _stream = null;

        private int lenght = 0;

        /// <summary>
        /// Handles new portion of data from a transport
        /// </summary>
        /// <param name="packetFromAStream">bytes from a transport stream</param>
        /// <param name="offset">position, where head starts</param>
        /// <returns>true if light-message is fully collected</returns>
        public bool Collect(byte[] packetFromAStream, int offset)
        {
            var head = packetFromAStream.ToStruct<PduHead>(offset, PduHead.DefaultHeadSize);
            
            int bodyStart = offset + DefaultHeadSize;
            int bodyLen = head.length - DefaultHeadSize;
            if (_stream == null)
            {
                if (head.type == PduType.Start)
                {
                    lenght = BitConverter.ToInt32(packetFromAStream, bodyStart);
                    _stream = new MemoryStream(lenght);
                    _stream.Write(packetFromAStream, bodyStart + 4, bodyLen - 4);
                }
                else
                {
                    throw new InvalidOperationException("Invalid pdu order");
                }
            }
            else if (head.type == PduType.Data)
            {
                _stream.Write(packetFromAStream, bodyStart, bodyLen);
            }
            else
            {
                _stream = null;
                return true;
            }
            if (_stream.Length == lenght)
                return true;
            if (_stream.Length < lenght)
                return false;

            _stream = null;
            return true;
        }


        /// <summary>
        /// Get collected message stream
        /// </summary>
        /// <returns></returns>
        public MemoryStream GetPduMessageStream()
        {
            return _stream;
        }
    }
}
