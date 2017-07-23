using System;
using System.Threading.Tasks;

namespace TNT.Transport
{
    public interface IChannel
    {
        /// <summary>
        /// Indicates connection status of downlayer TcpClient
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// Can Channel handle messages now?.
        /// </summary>
        bool AllowReceive { get; set; }
        /// <summary>
        /// Raising on new channel message received.
        /// It is blocking operation (ICHannel cannot handle other messages, while OnReceive handling)
        /// </summary>
        event Action<IChannel, byte[]> OnReceive;
        /// <summary>
        /// Raising if connection is lost
        /// </summary>
        event Action<IChannel> OnDisconnect;
        /// <summary>
        /// Close connection
        /// </summary>
        void Disconnect();

        Task<bool> TryWriteAsync(byte[] array);

        void Write(byte[] array);
    }
}
