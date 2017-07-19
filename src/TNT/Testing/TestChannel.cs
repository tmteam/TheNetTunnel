using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNT.Exceptions;

namespace TNT.Channel.Test
{
    public class TestChannel: IChannel
    {
        private bool _wasConnected;
        private bool _allowReceive;

        public void ImmitateReceive(byte[] message)
        {
            OnReceive?.Invoke(this, message);
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
            OnDisconnect?.Invoke(this);
        }

        public event Action<IChannel, byte[]> OnWrited;


        public bool IsConnected { get; private set; }

        public bool AllowReceive
        {
            get { return _allowReceive; }
            set
            {
                if(_allowReceive==value)
                    return;
                
                _allowReceive = value;
                AllowReceiveChanged?.Invoke(this,value);
            }
        }

        public event Action<IChannel, bool> AllowReceiveChanged; 
        public event Action<IChannel, byte[]> OnReceive;
        public event Action<IChannel> OnDisconnect;
        public void Disconnect()
        {
            if (IsConnected)
            {
                IsConnected = false;
                OnDisconnect?.Invoke(this);
            }
        }

        public Task<bool> TryWriteAsync(byte[] array)
        {
            return Task.Run(
                () => {
                    if (IsConnected)
                        Write(array);
                    return IsConnected;
                }
            );
        }

        public void Write(byte[] array)
        {
            if (!_wasConnected)
                throw new ConnectionIsNotEstablishedYet();
            if (!IsConnected)
                throw new ConnectionIsLostException();
            OnWrited?.Invoke(this, array);
        }
    }
}
