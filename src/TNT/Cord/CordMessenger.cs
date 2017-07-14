using System;
using System.Collections.Generic;
using System.IO;
using TNT.Cord.Deserializers;
using TNT.Cord.Serializers;
using TNT.Light;

namespace TNT.Cord
{
    public class CordMessenger: ICordMessenger
    {
        private readonly LightChannel _channel;

        private readonly Dictionary<int, OutputMessageSerializeInfo> _outputSayMessageSerializeInfos 
            = new Dictionary<int, OutputMessageSerializeInfo>();

        private readonly Dictionary<int, InputMessageDeserializeInfo> _inputSayMessageDeserializeInfos 
            = new Dictionary<int, InputMessageDeserializeInfo>();
        public event Action<ICordMessenger, int, int, object[]> OnAsk;
        public event Action<ICordMessenger, int, object[]> OnSay;
        public event Action<ICordMessenger, int, int, object> OnAns;

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

                _outputSayMessageSerializeInfos.Add(messageSayInfo.messageId, 
                    new OutputMessageSerializeInfo
                    {
                        Serializer = serializer,
                    });
                if (hasReturnType)
                {
                    _inputSayMessageDeserializeInfos.Add(
                        -messageSayInfo.messageId,
                        new InputMessageDeserializeInfo(1, false, deserializerFactory.Create(messageSayInfo.ReturnType)));
                }
            }
            foreach (var messageSayInfo in inputMessages)
            {
                var hasReturnType = messageSayInfo.ReturnType != typeof(void);

                var deserializer = deserializerFactory.Create(messageSayInfo.ArgumentTypes);
                _inputSayMessageDeserializeInfos.Add(
                    messageSayInfo.messageId,
                    new InputMessageDeserializeInfo(messageSayInfo.ArgumentTypes.Length, hasReturnType, deserializer));

                if (hasReturnType)
                {
                    _outputSayMessageSerializeInfos.Add(-messageSayInfo.messageId, new OutputMessageSerializeInfo
                    {
                        Serializer = serializerFactory.Create(messageSayInfo.ReturnType),
                    });
                }
            }
        }

      

        public void Say(int id, object[] values)
        {
            if (id < 0)
                throw new InvalidOperationException("say id < 0");

            var info =_outputSayMessageSerializeInfos[id];
            using (var stream = new MemoryStream())
            {
                CordTools.WriteShort((short) id, to: stream);
                Write(values, info, stream);
            }
        }

        public void Ans(short id, short askId, object value)
        {
            if (id >= 0)
                throw new InvalidOperationException("ans id >= 0");
            var info = _outputSayMessageSerializeInfos[id];

            using (var stream = new MemoryStream())
            {
                CordTools.WriteShort(id, to: stream);
                CordTools.WriteShort(askId, to: stream);
                info.Serializer.Serialize(value, stream);
                stream.Position = 0;
                _channel.Write(stream);
            }
        }

      

        public void Ask(short id, short askId, object[] values)
        {
            if (id < 0)
                throw new InvalidOperationException("ask id < 0");
            var info = _outputSayMessageSerializeInfos[id];

            using (var stream = new MemoryStream())
            {
                CordTools.WriteShort(id, to: stream);
                CordTools.WriteShort(askId, to: stream);
                Write(values, info, stream);
            }
        }

        private void Write(object[] values, OutputMessageSerializeInfo info, MemoryStream stream)
        {
            if (values.Length == 1)
                info.Serializer.Serialize(values[0], stream);
            else if (values.Length > 1)
                info.Serializer.Serialize(values, stream);
            stream.Position = 0;
            _channel.Write(stream);

        }

        private void _channel_OnReceive(LightChannel arg1, MemoryStream data)
        {
            var id = CordTools.ReadShort(data);
                InputMessageDeserializeInfo sayDeserializer;
                _inputSayMessageDeserializeInfos.TryGetValue(id, out sayDeserializer);
                if (sayDeserializer != null)
                {
                    if (id < 0)
                    {
                        //input answer message handling
                        var askId = CordTools.ReadShort(data);
                        var ans = sayDeserializer.Deserializer.Deserialize(data, (int)(data.Length - data.Position));
                        OnAns?.Invoke(this, id, askId, ans);
                        return;
                    }

                    if (sayDeserializer.HasReturnType)
                    {
                        //input ask messageHandling
                        var askId = CordTools.ReadShort(data);
                        var ans =
                            (object[])
                            sayDeserializer.Deserializer.Deserialize(data, (int) (data.Length - data.Position));
                        OnAsk?.Invoke(this, id, askId, ans);
                        return;
                    }
                    else
                    {
                        //input say messageHandling
                        object[] arg = null;
                        if(sayDeserializer.ArgumentsCount==0)
                            arg = new object[0];
                        else if(sayDeserializer.ArgumentsCount==1)
                            arg =  new []{ sayDeserializer.Deserializer.Deserialize(data, (int)(data.Length - data.Position)) };
                        else
                            arg =  (object[]) sayDeserializer.Deserializer.Deserialize(data, (int) (data.Length - data.Position));
                        OnSay?.Invoke(this, id, arg);
                        return;
                    }
                }
                throw new Exception($"Unknown id {id}");
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
        public InputMessageDeserializeInfo(int argumentsCount, bool hasReturnType, IDeserializer deserializer)
        {
            ArgumentsCount = argumentsCount;
            HasReturnType = hasReturnType;
            Deserializer = deserializer;
        }

        public int ArgumentsCount { get; }
        public IDeserializer Deserializer { get; }
        public bool HasReturnType { get; }
    }

    class OutputMessageSerializeInfo
    {
        public ISerializer Serializer;
    }
}