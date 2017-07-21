using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TNT.Cord.Deserializers;
using TNT.Cord.Serializers;
using TNT.Exceptions;
using TNT.Exceptions.ContractImplementation;
using TNT.Exceptions.Remote;
using TNT.Light;

namespace TNT.Cord
{
    public class CordMessenger : ICordMessenger
    {
        public const short ExceptionMessageId = 30400;
        public const short GeneralExceptionCordId = 0;


        private readonly LightChannel _channel;

        private readonly Dictionary<int, ISerializer> _outputSayMessageSerializes
            = new Dictionary<int, ISerializer>();

        private readonly Dictionary<int, InputMessageDeserializeInfo> _inputSayMessageDeserializeInfos
            = new Dictionary<int, InputMessageDeserializeInfo>();



        public event Action<ICordMessenger, short, short, object> OnAns;
        public event Action<ICordMessenger, ExceptionMessage> OnException;

        public CordMessenger(
            LightChannel channel,
            SerializerFactory serializerFactory,
            DeserializerFactory deserializerFactory,
            MessageTypeInfo[] outputMessages,
            MessageTypeInfo[] inputMessages)
        {
            _channel = channel;
            _channel.OnReceive += _channel_OnReceive;

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
            _inputSayMessageDeserializeInfos.Add(CordMessenger.ExceptionMessageId,
                InputMessageDeserializeInfo.CreateForExceptionHandling());
        }

        public void HandleCallException( RemoteExceptionBase rcException)
        {
            Say(CordMessenger.ExceptionMessageId,
                new object[] { new ExceptionMessage(rcException) },
                new ExceptionMessageSerializer());

            if (rcException.IsFatal)
                _channel.Disconnect();
        }

        public event Action<ICordMessenger, CordRequestMessage> OnRequest;

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
                CordTools.WriteShort(id, to: stream);
                CordTools.WriteShort(askId, to: stream);
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
                CordTools.WriteShort(id, to: stream);
                CordTools.WriteShort(askId, to: stream);
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
                CordTools.WriteShort((short) id, to: stream);
                Write(values, serializer, stream);
            }
        }

        private void _channel_OnReceive(LightChannel arg1, MemoryStream data)
        {
            if (data.Length - data.Position < 2)
            {
                HandleCallException(new RemoteSerializationException(
                     null, null, true,  "Cord id missed"));
                return;
            }
            var id = CordTools.ReadShort(data);
            InputMessageDeserializeInfo sayDeserializer;
            _inputSayMessageDeserializeInfos.TryGetValue(id, out sayDeserializer);
            if (sayDeserializer == null)
            {
                HandleCallException(new
                    RemoteContractImplementationException(
                    id, null, false, $"Cord message id{id} is not implemented"));
                return;
            }

            short? askId = null;
            if (id < 0 || sayDeserializer.HasReturnType)
            {
                if (data.Length - data.Position < 2)
                {
                    HandleCallException(new RemoteSerializationException(id, null, true, "Ask Id missed"));
                    return;
                }
                askId = CordTools.ReadShort(data);
            }
            object[] deserialized;

            try
            {
                deserialized = sayDeserializer.Deserialize(data);
            }
            catch (Exception ex)
            {
                HandleCallException(new RemoteSerializationException(id, askId, true,
                    $"Cord message id{id} with could not be deserialized. InnerException: {ex.ToString()}"));
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
            else if(id == CordMessenger.ExceptionMessageId)
            {
                var exceptionMessage = (ExceptionMessage)deserialized.First();
                if (exceptionMessage.Exception.IsFatal)
                    _channel.Disconnect();
                OnException?.Invoke(this, exceptionMessage);
            }
            else
            {
                //input ask / say messageHandling
                OnRequest?.Invoke(this, new CordRequestMessage(id, askId, deserialized));  
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