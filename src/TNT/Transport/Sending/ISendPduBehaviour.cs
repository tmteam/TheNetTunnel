using System.IO;

namespace TNT.Transport.Sending
{
    public interface ISendPduBehaviour
    {
        void Enqueue(MemoryStream lightMessage);
        bool TryDequeue(out byte[] quantum, out int messageId);
    }
}