using System;
using System.Threading.Tasks;
using TNT.Channel;

namespace TNT.Tests.Channel
{
    public class ChannelMock : IChannel
    {
        public bool IsConnected { get; }
        public bool AllowReceive { get; set; }

        public event Action<IChannel, byte[]> OnReceive;
        public event Action<IChannel> OnDisconnect;

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryWriteAsync(byte[] array)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] array)
        {
            throw new NotImplementedException();
        }
    }
}