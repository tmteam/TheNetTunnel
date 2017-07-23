using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TNT.Exceptions.ContractImplementation;
using TNT.Exceptions.Remote;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;
using TNT.Transport;

namespace TNT.Presentation
{
    public class Messenger : IMessenger
    {
        public const short ExceptionMessageTypeId = 30400;
        public const short GeneralExceptionMessageTypeId = 0;


        private readonly Transporter _channel;

        private readonly Dictionary<int, ISerializer> _outputSayMessageSerializes
            = new Dictionary<int, ISerializer>();

        private readonly Dictionary<int, InputMessageDeserializeInfo> _inputSayMessageDeserializeInfos
            = new Dictionary<int, InputMessageDeserializeInfo>();



        public event Action<IMessenger, short, short, object> OnAns;
        public event Action<IMessenger, ExceptionMessage> OnException;
        public event Action<IMessenger> ChannelIsDisconnected;

        public Messenger(
            Transporter channel,
            SerializerFactory serializerFactory,
            DeserializerFactory deserializerFactory,
            MessageTypeInfo[] outputMessages,
            MessageTypeInfo[] inputMessages)
        {
            _channel = channel;
            _channel.OnReceive += _channel_OnReceive;
            _channel.OnDisconnect += (c) => ChannelIsDisconnected?.Invoke(this);
            foreach (var messageSayInfo in outputMessages)
            {
                var serializer = serializerFactory.Create(messageSayInfo.ArgumentTypes);

                var hasReturnType = messageSayInfo.ReturnType != typeof(void);

                _outputSayMessageSerializes.Add(messageSayInfo.messageId, serializer);
                if (hasReturnType)
                {
                    _inputSayMessageDeserializeInfos.Add(
                        -messageSayInfo.messageId,
                        InputMessageDeserializeInfo.CreateForAnswer(deserializerFactory.Create(messageSayInfo.ReturnType)));
                }
            }
            foreach (var messageSayInfo in inputMessages)
            {
                var hasReturnType = messageSayInfo.ReturnType != typeof(void);

                var deserializer = deserializerFactory.Create(messageSayInfo.ArgumentTypes);
                _inputSayMessageDeserializeInfos.Add(
                    messageSayInfo.messageId,
                    InputMessageDeserializeInfo.CreateForAsk(messageSayInfo.ArgumentTypes.Length, hasReturnType,
                        deserializer));

                if (hasReturnType) {
                    _outputSayMessageSerializes.Add(-messageSayInfo.messageId,
                        serializerFactory.Create(messageSayInfo.ReturnType));
                }
            }
            _inputSayMessageDeserializeInfos.Add(Messenger.ExceptionMessageTypeId,
                InputMessageDeserializeInfo.CreateForExceptionHandling());
        }

     

        public void HandleCallException( RemoteExceptionBase rcException)
        {
            Say(Messenger.ExceptionMessageTypeId,
                new object[] { new ExceptionMessage(rcException) },
                new ExceptionMessageSerializer());

            if (rcException.IsFatal)
                _channel.Disconnect();
        }

        public event Action<IMessenger, RequestMessage> OnRequest;

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

        private void Say(int id, object[] values, ISerializer serializer)
        {
            using (var stream = new MemoryStream())
            {
                Tools.WriteShort((short) id, to: stream);
                Write(values, serializer, stream);
            }
        }

        private void _channel_OnReceive(Transporter arg1, MemoryStream data)
        {
            short id;
            if (!data.TryReadShort(out id))
            {
                HandleCallException(new RemoteSerializationException(
                    null, null, true, "Messae tyoe id missed"));
                return;
            }
            InputMessageDeserializeInfo sayDeserializer;
            _inputSayMessageDeserializeInfos.TryGetValue(id, out sayDeserializer);
            if (sayDeserializer == null)
            {
                HandleCallException(new
                    RemoteContractImplementationException(
                    id, data.TryReadShort(), false, $"Message type id {id} is not implemented"));
                return;
            }

            short? askId = null;
            if (id < 0 || sayDeserializer.HasReturnType)
            {
                askId = data.TryReadShort();
                if(!askId.HasValue)
                {
                    HandleCallException(new RemoteSerializationException(id, null, true, "Ask Id missed"));
                    return;
                }
            }
            object[] deserialized;

            try
            {
                deserialized = sayDeserializer.Deserialize(data);
            }
            catch (Exception ex)
            {
                HandleCallException(new RemoteSerializationException(id, askId, true,
                    $"Message type id{id} with could not be deserialized. InnerException: {ex.ToString()}"));
                return;
            }

            if (id < 0)
            {
                try
                {
                    //input answer message handling
                    OnAns?.Invoke(this, id, askId.Value, deserialized.Single());
                }
                catch (RemoteExceptionBase e)
                {
                    HandleCallException(e);
                }
               
            }
            else if(id == Messenger.ExceptionMessageTypeId)
            {
                var exceptionMessage = (ExceptionMessage)deserialized.First();
                if (exceptionMessage.Exception.IsFatal)
                    _channel.Disconnect();
                OnException?.Invoke(this, exceptionMessage);
            }
            else
            {
                //input ask / say messageHandling
                OnRequest?.Invoke(this, new RequestMessage(id, askId, deserialized));  
            }
        }
    
    }

    public class MessageTypeInfo
    {
        public Type[] ArgumentTypes;
        public Type ReturnType;
        public short messageId;
    }


    class InputMessageDeserializeInfo
    {
        public static InputMessageDeserializeInfo CreateForAnswer(IDeserializer deserializer)
        {
            return new InputMessageDeserializeInfo(1, false, deserializer);
        }

        public static InputMessageDeserializeInfo CreateForAsk(int argumentsCount, bool hasReturnType,
            IDeserializer deserializer)
        {
            return new InputMessageDeserializeInfo(argumentsCount, hasReturnType, deserializer);
        }

        public static InputMessageDeserializeInfo CreateForExceptionHandling()
        {
            return new InputMessageDeserializeInfo(1, false, new ExceptionMessageDeserializer());
        }

        private InputMessageDeserializeInfo(int argumentsCount, bool hasReturnType, IDeserializer deserializer)
        {
            ArgumentsCount = argumentsCount;
            HasReturnType = hasReturnType;
            Deserializer = deserializer;
        }


        public int ArgumentsCount { get; }
        public IDeserializer Deserializer { get; }
        public bool HasReturnType { get; }

        public object[] Deserialize(MemoryStream data)
        {
            object[] arg = null;
            if (ArgumentsCount == 0)
                arg = new object[0];
            else if (ArgumentsCount == 1)
                arg = new[] {Deserializer.Deserialize(data, (int) (data.Length - data.Position))};
            else
                arg = (object[]) Deserializer.Deserialize(data, (int) (data.Length - data.Position));
            return arg;
        }
    }


}