using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TNT.Transport.Sending
{
    /// <summary>
    /// Separate lightMessage into sequence of quants
    /// </summary>
    public class PacketSeparator
    {
        private static readonly int DefaultHeadSize = Marshal.SizeOf(typeof(PduHead));

        int dataLeft;

        public int DataLeft
        {
            get { return dataLeft; }
        }

        readonly int msgId;

        public int MessageId
        {
            get { return msgId; }
        }

        private readonly int _maxQuantSize;

        private readonly Stream _currentStream;

        bool got1Sended = false;

        /// <summary>
        /// Initialize separator with new stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="msgId"></param>
        /// <param name="maxQuantSize"></param>
        public PacketSeparator(Stream stream, int msgId, int maxQuantSize)
        {
            _maxQuantSize = maxQuantSize;
            dataLeft = (int) (stream.Length - stream.Position);
            this.msgId = msgId;
            got1Sended = false;
            _currentStream = stream;
        }

        public bool TryNext(out byte[] quant)
        {
            quant = null;
            if (got1Sended && dataLeft <= 0)
                return false;
            quant = Next();
            return true;
        }

        /// <summary>
        /// Get next quant
        /// </summary>
        private byte[] Next()
        {
            dataLeft = (int) (_currentStream.Length - _currentStream.Position);
            var actualHeadSize = got1Sended ? DefaultHeadSize : (DefaultHeadSize + 4);
            var head = new PduHead
            {
                length = (ushort) Math.Min(actualHeadSize + dataLeft, _maxQuantSize),
                msgId = msgId,
                type = got1Sended ? PduType.Data : PduType.Start,
            };

            byte[] Quant = new byte[head.length];

            //Head
            head.SetToArray(Quant, 0, DefaultHeadSize);

            //Length if start quant
            if (!got1Sended)
                Array.Copy(BitConverter.GetBytes(dataLeft), 0, Quant, DefaultHeadSize, 4);

            //data
            _currentStream.Read(Quant, actualHeadSize, head.length - actualHeadSize);

            got1Sended = true;
            dataLeft -= (Quant.Length - actualHeadSize);
            return Quant;
        }
    }
}


