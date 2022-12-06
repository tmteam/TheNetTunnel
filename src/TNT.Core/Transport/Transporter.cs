using System;
using System.IO;
using System.Threading.Tasks;
using TNT.Exceptions.Local;
using TNT.Presentation;

namespace TNT.Transport;

public class Transporter
{
    private readonly ReceivePduQueue _receiveMessageAssembler;
    private readonly SendStreamManager _sendStreamManager = new SendStreamManager();
    public Transporter(IChannel underlyingChannel)
    {
        _receiveMessageAssembler = new ReceivePduQueue();
        Channel = underlyingChannel;
        underlyingChannel.OnDisconnect += (s,e) => OnDisconnect?.Invoke(this,e);
        underlyingChannel.OnReceive += UnderlyingChannel_OnReceive;
    }

    public bool IsConnected => Channel.IsConnected;
    public IChannel Channel { get; }
    public bool AllowReceive { get => Channel.AllowReceive;
        set => Channel.AllowReceive = value;
    }
      
    public event Action<Transporter, MemoryStream> OnReceive;
    public event Action<Transporter, ErrorMessage> OnDisconnect;

    public void DisconnectBecauseOf(ErrorMessage error) {
        Channel.DisconnectBecauseOf(error);
    }
       
    public void Disconnect() {
        Channel.Disconnect();
    }

    /// <summary>
    /// Sends the stream as a packet
    /// </summary>
    /// <param name="message"></param>
    ///<exception cref="ConnectionIsLostException"></exception>
    public void Write(MemoryStream message)
    {
        _sendStreamManager.PrepareForSending(message);
        if (!message.TryGetBuffer(out var buffer))
            throw new InvalidOperationException();
        Channel.Write(buffer.Array, (int)message.Position, (int)(message.Length - message.Position));
    }

      

    public MemoryStream CreateStreamForSend()
    {
        return _sendStreamManager.CreateStreamForSend();
    }
    /// <summary>
    /// Sends the stream as a packet
    /// </summary>
    /// <param name="packet"></param>
    ///<exception cref="ConnectionIsLostException"></exception>
    public async Task WriteAsync(MemoryStream packet)
    {
        _sendStreamManager.PrepareForSending(packet);
        if (!packet.TryGetBuffer(out var buffer))
            throw new InvalidOperationException();

        Channel.Write(buffer.Array, 
            (int)packet.Position,
            (int)(packet.Length - packet.Position));
    }
    private void UnderlyingChannel_OnReceive(object arg1, byte[] data)
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