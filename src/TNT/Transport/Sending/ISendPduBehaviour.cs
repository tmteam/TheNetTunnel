using System.Collections.Generic;
using System.IO;

namespace TNT.Transport.Sending
{
    public interface ISendPduBehaviour
    {
        void Enqueue(MemoryStream lightMessage);
        IEnumerable<byte[]> TryDequeue();
    }
}