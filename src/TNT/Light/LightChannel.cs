using System;
using System.IO;
using System.Threading.Tasks;
using TNT.Light.Receiving;
using TNT.Light.Sending;

namespace TNT.Light
{
    public class LightChannel
    {
        private readonly ISendMessageSequenceBehaviour _sendMessageSeparatorBehaviour;
        private readonly ReceiveMessageQueue _receiveMessageAssembler;

        public LightChannel(IChannel underlyingChannel, 
            ISendMessageSequenceBehaviour sendMessageSequenceBehaviour)
        {
            _sendMessageSeparatorBehaviour = sendMessageSequenceBehaviour;
            _receiveMessageAssembler = new ReceiveMessageQueue();
            Channel = underlyingChannel;
            underlyingChannel.OnDisconnect += (s) => OnDisconnect?.Invoke(this);
            underlyingChannel.OnReceive += UnderlyingChannel_OnReceive;
        }

        public bool IsConnected => Channel.IsConnected;

        public IChannel Channel { get; }

        public bool AllowReceive { get { return Channel.AllowReceive; } set { Channel.AllowReceive = value; } }

      
        public event Action<LightChannel, MemoryStream> OnReceive;
        public event Action<LightChannel> OnDisconnect;

        public void Disconnect()
        {
            Channel.Disconnect();
        }

        public async Task<bool>  TryWriteAsync(MemoryStream stream)
        {
            _sendMessageSeparatorBehaviour.Enqueue(stream);
            int id;
            byte[] msg;
            while (_sendMessageSeparatorBehaviour.TryDequeue(out msg, out id))
            {
                var result = await Channel.TryWriteAsync(msg);
                if (!result)
                    return false;
            }
            return true;
        }

        public bool Write(MemoryStream stream)
        {
            _sendMessageSeparatorBehaviour.Enqueue(stream);
            int id;
            byte[] msg;
            while (_sendMessageSeparatorBehaviour.TryDequeue(out msg, out id))
            {
                  Channel.Write(msg);
            }
            return true;
        }


        private void UnderlyingChannel_OnReceive(IChannel arg1, byte[] data)
        {
            _receiveMessageAssembler.Enqueue(data);
            while (true)
            {
                var message = _receiveMessageAssembler.DequeueOrNull();
                if (message == null)
                    return;
                OnReceive?.Invoke(this, message);
            }
        }
    }
}
