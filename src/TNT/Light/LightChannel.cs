using System;
using System.IO;
using System.Threading.Tasks;
using TNT.Channel;
using TNT.Light.Receiving;
using TNT.Light.Sending;

namespace TNT.Light
{
    public class LightChannel
    {
        private readonly ISendMessageSequenceBehaviour _sendMessageSeparatorBehaviour;
        private readonly IDispatcher _receiveMessageThreadBehavior;
        private readonly ReceiveMessageQueue _receiveMessageAssembler;

        public LightChannel(IChannel underlyingChannel, 
            ISendMessageSequenceBehaviour sendMessageSequenceBehaviour,
            IDispatcher receiveMessageThreadBehavior)
        {
            _sendMessageSeparatorBehaviour = sendMessageSequenceBehaviour;
            _receiveMessageThreadBehavior = receiveMessageThreadBehavior;
            _receiveMessageAssembler = new ReceiveMessageQueue();
            _receiveMessageThreadBehavior.OnNewMessage += _receiveMessageThreadBehavior_OnNewMessage;
            Channel = underlyingChannel;
            underlyingChannel.OnDisconnect += (s) => OnDisconnect?.Invoke(this);
            underlyingChannel.OnReceive += UnderlyingChannel_OnReceive;
        }

        private void _receiveMessageThreadBehavior_OnNewMessage(IDispatcher sender, MemoryStream message)
        {
            OnReceive?.Invoke(this, message);
        }

        public bool IsConnected => Channel.IsConnected;

        public IChannel Channel { get; }

        public bool AllowReceive { get { return Channel.AllowReceive; } set { Channel.AllowReceive = value; } }

      
        public event Action<LightChannel, MemoryStream> OnReceive;
        public event Action<LightChannel> OnDisconnect;

        public void Disconnect()
        {
            _receiveMessageThreadBehavior.Release();
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
                if(Channel.IsConnected)
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
                _receiveMessageThreadBehavior.Set(message);
            }
        }
    }
}
