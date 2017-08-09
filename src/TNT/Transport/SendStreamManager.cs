using System.IO;

namespace TNT.Transport
{
    public class SendStreamManager
    {
        public MemoryStream CreateStreamForSend()
        {
            var stream = new MemoryStream(1024);

            if (ReservedHeadLength > 0)
                stream.Write(new byte[ReservedHeadLength], 0, ReservedHeadLength);
            return stream;
        }
        public int ReservedHeadLength => sizeof(uint);

        public void PrepareForSending(MemoryStream message)
        {
            message.Position = 0;
            uint len = (uint)(message.Length - ReservedHeadLength);
            message.WriteInt(len);

            message.Position = 0;
        }
    }
}