using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TNT.Transport
{
    public class ReceivePduQueue
    {
        private const int LengthHeadeLength = sizeof(int);

        private readonly Queue<MemoryStream> _queue = new Queue<MemoryStream>();

        private MemoryStream _collectingPacket = null;
        private int _awaitedLength = 0;
        private readonly List<byte> _undoneheader = new List<byte>(LengthHeadeLength);
        void Enqueue(byte[] data, int offset)
        {
            if(data.Length==offset)
                return;

            if (_collectingPacket != null)
            {
                ContinueHandlePacket(data, offset);
                return;
            }

            int left = data.Length - offset;

            if (_undoneheader.Count == 0)
            {
                if (left >= LengthHeadeLength)
                {
                    _awaitedLength = data.ToStruct<int>(offset);
                    StartHandlePacket(data, offset + LengthHeadeLength);
                }
                else
                {
                    _undoneheader.AddRange(data.Skip(offset));
                }
                return;
            }

            var awaitOfHead = LengthHeadeLength - _undoneheader.Count;
            if (awaitOfHead <= left)
            {
                _undoneheader.AddRange(data.Skip(offset).Take(awaitOfHead));
                _awaitedLength = _undoneheader.ToArray().ToStruct<int>(0);
                _undoneheader.Clear();
                StartHandlePacket(data, offset + awaitOfHead);
            }
            else
            {
                _undoneheader.AddRange(data.Skip(offset));
            }
        }

        private void StartHandlePacket(byte[] data, int offset)
        {
            _collectingPacket = new MemoryStream(_awaitedLength);
            ContinueHandlePacket(data, offset);
        }

        private void ContinueHandlePacket(byte[] data, int dataOffset)
        {
            int dataLeft = data.Length - dataOffset;

            if (dataLeft < _awaitedLength)
            {
                _collectingPacket.Write(data, dataOffset, dataLeft);
                _awaitedLength -= dataLeft;
            }
            else
            {
                //packet finished.
                _collectingPacket.Write(data, dataOffset, _awaitedLength);
                _collectingPacket.Position = 0;
                _queue.Enqueue(_collectingPacket);
                _collectingPacket = null;
                var nextPackOffset = dataOffset + _awaitedLength;
                _awaitedLength = 0;
                Enqueue(data, nextPackOffset);
            }
        }

        public void Enqueue(byte[] data)
        {
            Enqueue(data, 0);
        }
        
        public bool IsEmpty => _queue.Count == 0;

        public MemoryStream DequeueOrNull()
        {
            if(_queue.Count>0)
                return _queue.Dequeue();
            return null;
        }
    }
}
