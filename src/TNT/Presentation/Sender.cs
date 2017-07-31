using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.Serializers;
using TNT.Transport;

namespace TNT.Presentation
{
    /// <summary>
    /// Implements ouput tnt operations
    /// </summary>
    public class Sender
    {
        private readonly Transporter _channel;
        private readonly Dictionary<int, ISerializer> _outputSayMessageSerializes;

        public Sender(Transporter channel, Dictionary<int, ISerializer> outputSayMessageSerializes)
        {
            _channel = channel;
            _outputSayMessageSerializes = outputSayMessageSerializes;
        }
        /// <summary>
        /// Sends "Say" message with "values" arguments
        /// </summary>
        /// <exception cref="ArgumentException">wrong messageId</exception>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">specified serializer is missing or does not fit the arguments</exception>
        public void Say(int messageId, object[] values)
        {
            if (messageId < 0)
                throw new ArgumentException("Say id < 0");
            ISerializer serializer;
            _outputSayMessageSerializes.TryGetValue(messageId, out serializer);
            if(serializer==null)
                throw new ArgumentException($"Say-message id {messageId} is unknown");

            Say(messageId, values, serializer);
        }
        /// <summary>
        /// 
        /// </summary>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="ArgumentException">Occurs when messageId is wrong</exception>
        ///<exception cref="LocalSerializationException">argument type serializer is not implemented, or not the same as specified during the construction</exception>
        public void Ans(short messageId, short askId, object value)
        {
            if (messageId >= 0)
                throw new ArgumentException("Ans-messageId have to be < 0");

            ISerializer serializer;
            _outputSayMessageSerializes.TryGetValue(messageId, out serializer);
            if (serializer == null)
                throw new ArgumentException($"Ans-messageId {messageId} is unknown");


            var stream = new MemoryStream();
            Tools.WriteShort(messageId, to: stream);
            Tools.WriteShort(askId, to: stream);
            try
            {
                serializer.Serialize(value, stream);
            }
            catch (Exception e)
            {
                throw new LocalSerializationException(messageId,askId,"Serialization failed because of: "+ e.Message,e);
            }
            stream.Position = 0;
            _channel.Write(stream);
        }
        /// <summary>
        /// Sends error message
        /// </summary>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">one of the argument type serializers is not implemented, or not the same as specified during the construction</exception>
        ///<exception cref="ArgumentException">Occurs when messageId is wrong</exception>
        public void Ask(short id, short askId, object[] values)
        {
            if (id < 0)
                throw new ArgumentException("Ask-messageId has to be positive");

            ISerializer serializer;
            _outputSayMessageSerializes.TryGetValue(id, out serializer);
            if (serializer == null)
                throw new ArgumentException($"Ask-messageId {id} is unknown");

            var stream = new MemoryStream();
            Tools.WriteShort(id, to: stream);
            Tools.WriteShort(askId, to: stream);
            Write(values, serializer, stream);
            
        }

        /// <summary>
        /// Sends error message
        /// </summary>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">specified serializer does not fit the arguments</exception>
        public void SendError(ErrorMessage errorInfo)
        {
            var stream = new MemoryStream();
            Tools.WriteShort((short)Messenger.ExceptionMessageTypeId, to: stream);
            new ErrorMessageSerializer().SerializeT(errorInfo, stream);
            stream.Position = 0;
            _channel.WriteAsync(stream).Wait();
        }
        /// <summary>
        /// Serializes data to the stream and sends it 
        /// </summary>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">specified serializer does not fit the arguments</exception>
        private void Write(object[] values, ISerializer serializer, MemoryStream stream)
        {
            try
            {
                if (values.Length == 1)
                    serializer.Serialize(values[0], stream);
                else if (values.Length > 1)
                    serializer.Serialize(values, stream);
            }
            catch (Exception e)
            {
                _channel.Disconnect();
                throw new LocalSerializationException(null,null,"Serialization failed", e);
            }
          
            stream.Position = 0;
            _channel.Write(stream);
        }
       
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">specified serializer does not fit the arguments</exception>
        private void Say(int id, object[] values, ISerializer serializer)
        {
            var stream = new MemoryStream();
            Tools.WriteShort((short)id, to: stream);
            Write(values, serializer, stream);
        }
    }
}
