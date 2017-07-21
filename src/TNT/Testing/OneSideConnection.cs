using System.Collections.Concurrent;
using System.IO;
using TNT.Light;

namespace TNT.Testing
{
    class OneSideConnection
    {
        public TestChannel CahnnelFrom { get; }
        public TestChannel ChannelTo { get; }
        public bool IsStarted { get; set; }
        private ConcurrentQueue<byte[]> _receiveBuffer = new ConcurrentQueue<byte[]>();

        public OneSideConnection(TestChannel cahnnelFrom, TestChannel channelTo)
        {
            ChannelTo = channelTo;
            CahnnelFrom = cahnnelFrom;
        }

        private void ChannelTo_AllowReceiveChanged(IChannel arg1, bool allowReceive)
        {
            if (allowReceive)
                WriteAll();
        }

        private void CahnnelFrom_OnWrited(IChannel arg1, byte[] arg2)
        {
            if (!ChannelTo.IsConnected)
                throw new IOException("Remote Test Channel is Disconnected");
            if (!CahnnelFrom.IsConnected)
                throw new IOException("Test Channel is Disconnected");
            _receiveBuffer.Enqueue(arg2);

            if (ChannelTo.AllowReceive)
            {
                WriteAll();
            }
        }

        private void WriteAll()
        {
            while (!_receiveBuffer.IsEmpty)
            {
                byte[] msg;
                _receiveBuffer.TryDequeue(out msg);
                ChannelTo.ImmitateReceive(msg);
            }
        }

        public void Start()
        {
            IsStarted = true;
            CahnnelFrom.OnWrited += CahnnelFrom_OnWrited;
            ChannelTo.AllowReceiveChanged += ChannelTo_AllowReceiveChanged;
        }

        public void Stop()
        {
            _receiveBuffer = new ConcurrentQueue<byte[]>();

            IsStarted = false;
            CahnnelFrom.OnWrited -= CahnnelFrom_OnWrited;
            ChannelTo.AllowReceiveChanged -= ChannelTo_AllowReceiveChanged;
        }
    }
}