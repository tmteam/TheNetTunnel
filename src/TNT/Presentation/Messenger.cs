using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TNT.Exceptions.ContractImplementation;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;
using TNT.Transport;

namespace TNT.Presentation
{
    /// <summary>
    /// Incapsulates basic Tnt IO operations 
    /// </summary>
    public class Messenger : IMessenger
    {
        public const short ExceptionMessageTypeId = 30400;
        public const short GeneralExceptionMessageTypeId = 0;

        private readonly Sender _sender;
        private readonly Transporter _channel;

        private readonly Dictionary<int, InputMessageDeserializeInfo> _inputSayMessageDeserializeInfos
            = new Dictionary<int, InputMessageDeserializeInfo>();
        
        public event Action<IMessenger, short, short, object> OnAns;
        public event Action<IMessenger, ErrorMessage> OnRemoteError;
        public event Action<IMessenger, ErrorMessage> ChannelIsDisconnected;
        public event Action<IMessenger, RequestMessage> OnRequest;

        public Messenger(
            Transporter channel,
            SerializerFactory serializerFactory,
            DeserializerFactory deserializerFactory,
            MessageTypeInfo[] outputMessages,
            MessageTypeInfo[] inputMessages)
        {
            _channel = channel;
            _channel.OnReceive += _channel_OnReceive;
            _channel.OnDisconnect += (c,e) => ChannelIsDisconnected?.Invoke(this,e);


            var outputSayMessageSerializes = new Dictionary<int, ISerializer>();

            foreach (var messageSayInfo in outputMessages)
            {
                var serializer = serializerFactory.Create(messageSayInfo.ArgumentTypes);
                var hasReturnType = messageSayInfo.ReturnType != typeof(void);

                outputSayMessageSerializes.Add(messageSayInfo.MessageId, serializer);
                if (hasReturnType)
                {
                    _inputSayMessageDeserializeInfos.Add(
                        -messageSayInfo.MessageId,
                        InputMessageDeserializeInfo.CreateForAnswer(deserializerFactory.Create(messageSayInfo.ReturnType)));
                }
            }
            foreach (var messageSayInfo in inputMessages)
            {
                var hasReturnType = messageSayInfo.ReturnType != typeof(void);
                var deserializer = deserializerFactory.Create(messageSayInfo.ArgumentTypes);
                _inputSayMessageDeserializeInfos.Add(
                    messageSayInfo.MessageId,
                    InputMessageDeserializeInfo.CreateForAsk(messageSayInfo.ArgumentTypes.Length, hasReturnType,
                        deserializer));

                if (hasReturnType) {
                    outputSayMessageSerializes.Add(-messageSayInfo.MessageId,
                        serializerFactory.Create(messageSayInfo.ReturnType));
                }
            }
            _inputSayMessageDeserializeInfos.Add(Messenger.ExceptionMessageTypeId,
                InputMessageDeserializeInfo.CreateForExceptionHandling());

            _sender = new Sender(_channel, outputSayMessageSerializes);
        }
        /// <summary>
        /// Handles the error, occured during the input message handling.
        /// try to send an error message to remote side
        /// </summary>
        ///<exception cref="InvalidOperationException">Critical implementation exception</exception>
        public void HandleRequestProcessingError(ErrorMessage errorInfo, bool isFatal)
        {
            if (!_channel.IsConnected)
                return;
            try
            {
                _sender.SendError(errorInfo);
                if (isFatal)
                    _channel.DisconnectBecauseOf(errorInfo);
            }
            catch (LocalSerializationException e)
            {
                throw new InvalidOperationException("Error-serializer exception", e);
            }
            catch (ConnectionIsNotEstablishedYet)
            {
                throw new InvalidOperationException("Channel implementation error. Chanel " + _channel.GetType().Name +
                                                    " throws ConnectionIsNotEstablishedYet when it was connected");
            }
            catch (ConnectionIsLostException)
            {
                _channel.Disconnect();
            }
        }

        /// <summary>
        /// Sends "Say" message with "values" arguments
        /// </summary>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">one of the argument type serializers is not implemented, or not the same as specified in the contract</exception>
        public void Say(short messageId, object[] values) {
            _sender.Say(messageId, values);
        }
        /// <summary>
        /// Sends "ans value" message 
        /// </summary>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">answer type serializer is not implemented, or  not the same as specified in the contract</exception>
        public void Ans(short id, short askId, object value) {
           _sender.Ans(id,askId,value);
        }
        /// <summary>
        /// Sends "Say" message with "values" arguments
        /// </summary>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ConnectionIsLostException"></exception>
        ///<exception cref="LocalSerializationException">one of the argument type serializers is not implemented, or not the same as specified in the contract</exception>
        public void Ask(short id, short askId, object[] values) {
            _sender.Ask(id, askId, values);
        }

        private void _channel_OnReceive(Transporter arg1, MemoryStream data)
        {
            short id;
            if (!data.TryReadShort(out id))
            {
                HandleRequestProcessingError(
                    new ErrorMessage(
                        null,null, 
                        ErrorType.SerializationError, 
                        "Messae type id missed"),true);
                 
                return;
            }
            InputMessageDeserializeInfo sayDeserializer;
            _inputSayMessageDeserializeInfos.TryGetValue(id, out sayDeserializer);
            if (sayDeserializer == null)
            {
                HandleRequestProcessingError(
                    new ErrorMessage(id, data.TryReadShort(),
                        ErrorType.ContractSignatureError,
                        $"Message type id {id} is not implemented"), false);
                return;
            }

            short? askId = null;
            if (id < 0 || sayDeserializer.HasReturnType)
            {
                askId = data.TryReadShort();
                if(!askId.HasValue)
                {
                    HandleRequestProcessingError(
                         new ErrorMessage(
                            id, null,
                            ErrorType.SerializationError,
                            "Ask Id missed"), true);
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
                HandleRequestProcessingError(
                        new ErrorMessage(
                           id, askId,
                           ErrorType.SerializationError,
                           $"Message type id{id} with could not be deserialized. InnerException: {ex.ToString()}"), true);
                return;
            }

            if (id < 0)
            {
                //input answer message handling
                OnAns?.Invoke(this, id, askId.Value, deserialized.Single());
                
            }
            else if(id == Messenger.ExceptionMessageTypeId)
            {
                var exceptionMessage = (ErrorMessage)deserialized.First();
                if (exceptionMessage.Exception.IsFatal)
                    _channel.DisconnectBecauseOf(exceptionMessage);
                OnRemoteError?.Invoke(this, exceptionMessage);
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
        public short MessageId;
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
            return new InputMessageDeserializeInfo(1, false, new ErrorMessageDeserializer());
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