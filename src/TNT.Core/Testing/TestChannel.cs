using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TNT.Exceptions.Local;
using TNT.Presentation;
using TNT.Transport;

namespace TNT.Testing;

public class TestChannel: IChannel
{
    private readonly bool _threadQueue;
    private bool _wasConnected;
    private bool _allowReceive;
    readonly ConcurrentQueue<byte[]> _receiveQueue = new ConcurrentQueue<byte[]>();
    private  int _bytesReceived;
    private int _bytesSent;

    public TestChannel(bool threadQueue = true)
    {
        _threadQueue = threadQueue;
    }
    public void ImmitateReceive(byte[] message)
    {
            
        _receiveQueue.Enqueue(message);
        if(_threadQueue)
        {
            newDataReveived.Set();
        }
        else
            HandleReceiveQueue();
    }

    readonly AutoResetEvent newDataReveived = new AutoResetEvent(false);
    Thread receiveThreadOrNull;
    void ThreadVoid()
    {
        while (IsConnected)
        {
            newDataReveived.WaitOne(100);
            HandleReceiveQueue();
        }
    }

    void HandleReceiveQueue()
    {
        _receiveQueue.TryDequeue(out var msg);
        if(msg==null)
            return;
        if(!IsConnected)
            return;
        _bytesReceived += msg.Length;
        OnReceive?.Invoke(this, msg);
    }
    public void ImmitateConnect()
    {
        if(IsConnected)
            throw  new InvalidOperationException("Cannot to immitate connect while IsConnected = true");
        _wasConnected = true;
        IsConnected = true;
    }

    public void ImmitateDisconnect()
    {
        if (!IsConnected)
            throw new InvalidOperationException("Cannot to immitate disconnect while IsConnected = false");
        IsConnected = false;
        OnDisconnect?.Invoke(this, null);
    }

    public event Action<object, byte[]> OnWrited;

    public bool IsConnected { get; private set; }

    public bool AllowReceive

    {
        get => _allowReceive;
        set
        {
            if(_allowReceive==value)
                return;
                
            _allowReceive = value;

            if (_allowReceive)
            {
                if (!_threadQueue)
                    HandleReceiveQueue();
                else
                {
                    this.receiveThreadOrNull = new Thread((s) => ThreadVoid());
                    receiveThreadOrNull.Start();
                }
            }
            AllowReceiveChanged?.Invoke(this,value);
        }
    }

    public event Action<IChannel, bool> AllowReceiveChanged; 
    public event Action<object, byte[]> OnReceive;
    public event Action<object, ErrorMessage> OnDisconnect;
    public void Disconnect()
    {
        DisconnectBecauseOf(null);
    }
    public void DisconnectBecauseOf(ErrorMessage error)
    {
        if (IsConnected)
        {
            IsConnected = false;
            OnDisconnect?.Invoke(this, error);
        }
    }
    public Task<bool> TryWriteAsync(byte[] array)
    {
        return Task.Run(
            () => {
                if (IsConnected)
                    Write(array, 0, array.Length);
                return IsConnected;
            }
        );
    }

    public void Write(byte[] array, int offset, int length)
    {
        if (!_wasConnected)
            throw new ConnectionIsNotEstablishedYet();
        if (!IsConnected)
            throw new ConnectionIsLostException();

        Interlocked.Add(ref _bytesSent, length);

        var buf = new byte[length];
        Buffer.BlockCopy(array, offset, buf, 0, length);

        OnWrited?.Invoke(this, buf);
    }

    public int BytesReceived => _bytesReceived;

    public int BytesSent => _bytesSent;

    public string RemoteEndpointName { get; }
    public string LocalEndpointName { get; }
    public Task WriteAsync(byte[] data)
    {
        return Task.Run(() => Write(data,0, data.Length));
    }
}