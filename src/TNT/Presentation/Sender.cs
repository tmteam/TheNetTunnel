using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNT.Exceptions.Remote;
using TNT.Presentation.Serializers;
using TNT.Transport;

namespace TNT.Presentation
{
    public class Sender
    {
        private readonly Transporter _channel;
        private readonly Dictionary<int, ISerializer> _outputSayMessageSerializes;

        public Sender(Transporter channel, Dictionary<int, ISerializer> outputSayMessageSerializes)
        {
            _channel = channel;
            _outputSayMessageSerializes = outputSayMessageSerializes;
        }

        public void Say(int id, object[] values)
        {
            if (id < 0)
                throw new InvalidOperationException("say id < 0");

            var info = _outputSayMessageSerializes[id];
            Say(id, values, info);
        }

        public void Ans(short id, short askId, object value)
        {
            if (id >= 0)
                throw new InvalidOperationException("ans id >= 0");
            var serializer = _outputSayMessageSerializes[id];

            using (var stream = new MemoryStream())
            {
                Tools.WriteShort(id, to: stream);
                Tools.WriteShort(askId, to: stream);
                serializer.Serialize(value, stream);
                stream.Position = 0;
                _channel.Write(stream);
            }
        }

        public void Ask(short id, short askId, object[] values)
        {
            if (id < 0)
                throw new InvalidOperationException("ask id < 0");
            var info = _outputSayMessageSerializes[id];

            using (var stream = new MemoryStream())
            {
                Tools.WriteShort(id, to: stream);
                Tools.WriteShort(askId, to: stream);
                Write(values, info, stream);
            }
        }

        private void Write(object[] values, ISerializer serializer, MemoryStream stream)
        {
            if (values.Length == 1)
                serializer.Serialize(values[0], stream);
            else if (values.Length > 1)
                serializer.Serialize(values, stream);
            stream.Position = 0;
            _channel.Write(stream);

        }

        public void SendException(RemoteExceptionBase rcException)
        {
            
        }
        public void Say(int id, object[] values, ISerializer serializer)
        {
            using (var stream = new MemoryStream())
            {
                Tools.WriteShort((short)id, to: stream);
                Write(values, serializer, stream);
            }
        }

    }
}
