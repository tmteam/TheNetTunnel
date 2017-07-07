using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TNT.Light.Receiving
{
    /// <summary>
    /// Collect single light message from quants
    /// </summary>
    public class MessageCollector
    {
        private static readonly int DefaultHeadSize = Marshal.SizeOf(typeof(QuantumHead));

        //public DateTime LastTimeSet { get; private set; }

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
            //LastTimeSet = DateTime.Now;

            var head = packetFromAStream.ToStruct<QuantumHead>(offset, QuantumHead.DefaultHeadSize);


            int bodyStart = offset + DefaultHeadSize;
            int bodyLen = head.length - DefaultHeadSize;
            if (_stream == null)
            {
                if (head.type == QuantumType.Start)
                {
                    lenght = BitConverter.ToInt32(packetFromAStream, bodyStart);
                    _stream = new MemoryStream(lenght);
                    _stream.Write(packetFromAStream, bodyStart + 4, bodyLen - 4);
                }
                else
                {
                    throw new InvalidOperationException("Invalid quant order");
                }
            }
            else if (head.type == QuantumType.Data)
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

        ///// <summary>
        /////     reset collector
        ///// </summary>
        //public void Clear()
        //{
        //    _stream = null;
        //    lenght = 0;
        //}

        /// <summary>
        /// Get collected message stream
        /// </summary>
        /// <returns></returns>
        public MemoryStream GetLightMessageStream()
        {
            return _stream;
        }
    }
}
