using System;
using System.IO;
using System.Threading.Tasks;
using TNT.Exceptions.Local;
using TNT.Presentation;
using TNT.Transport.Receiving;
using TNT.Transport.Sending;

namespace TNT.Transport
{
    public class Transporter
    {
        private readonly ISendPduBehaviour _sendMessageSeparatorBehaviour;
        private readonly ReceivePduQueue _receiveMessageAssembler;

        public Transporter(IChannel underlyingChannel, 
            ISendPduBehaviour sendMessageSequenceBehaviour)
        {
            _sendMessageSeparatorBehaviour = sendMessageSequenceBehaviour;
            _receiveMessageAssembler = new ReceivePduQueue();
            Channel = underlyingChannel;
            underlyingChannel.OnDisconnect += (s,e) => OnDisconnect?.Invoke(this,e);
            underlyingChannel.OnReceive += UnderlyingChannel_OnReceive;
        }

        public bool IsConnected => Channel.IsConnected;

        public IChannel Channel { get; }

        public bool AllowReceive { get { return Channel.AllowReceive; } set { Channel.AllowReceive = value; } }

      
        public event Action<Transporter, MemoryStream> OnReceive;
        public event Action<Transporter, ErrorMessage> OnDisconnect;

        public void DisconnectBecauseOf(ErrorMessage error)
        {
            Channel.DisconnectBecauseOf(error);
        }
       
        public void Disconnect()
        {
            Channel.Disconnect();
        }

        /// <summary>
        /// Sends the stream as a packet
        /// </summary>
        /// <param name="packet"></param>
        ///<exception cref="ConnectionIsLostException"></exception>
        public void Write(MemoryStream packet)
        {
            _sendMessageSeparatorBehaviour.Enqueue(packet);
            int id;
            byte[] msg;
            while (_sendMessageSeparatorBehaviour.TryDequeue(out msg, out id))
            {
                Channel.Write(msg);
            }
        }

        /// <summary>
        /// Sends the stream as a packet
        /// </summary>
        /// <param name="packet"></param>
        ///<exception cref="ConnectionIsLostException"></exception>
        public async Task WriteAsync(MemoryStream packet)
        {
            _sendMessageSeparatorBehaviour.Enqueue(packet);
            int id;
            byte[] msg;
            while (_sendMessageSeparatorBehaviour.TryDequeue(out msg, out id))
            {
                await Channel.WriteAsync(msg);
            }
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
