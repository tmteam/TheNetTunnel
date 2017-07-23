using System;
using System.Linq;
using TNT.Contract;
using TNT.Contract.Origin;
using TNT.Contract.Proxy;
using TNT.Presentation;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;
using TNT.Transport;

namespace TNT.Api
{
    public class ConnectionBuilder<TContract, TChannel> 
            where TChannel : IChannel
            where TContract: class 
    {
        private readonly PresentationBuilder<TContract> _contractBuilder;
        private readonly Func<TChannel> _channelFactory;

        public  ConnectionBuilder(PresentationBuilder<TContract> contractBuilder, Func<TChannel> channelFactory)
        {
            _contractBuilder = contractBuilder;
            _channelFactory = channelFactory;
        }
        public Connection<TContract, TChannel> Build()
        {
            
            var sendSeparationBehaviour = _contractBuilder.SendMessageSequenceBehaviourFactory();
            var channel = _channelFactory();

            var light = new Transporter(
                underlyingChannel: channel,
                sendMessageSequenceBehaviour: sendSeparationBehaviour);

            TContract contract = null;
            if (_contractBuilder.OriginContractFactory == null)
            {
                contract = CreateProxyContract(light);
            }
            else
            {
                contract = CreateOriginContract(light);
            }

            _contractBuilder.ContractInitializer(contract, channel);
            if(channel.IsConnected)
                channel.AllowReceive = true;

            return new Connection<TContract, TChannel>(contract, channel, _contractBuilder.ContractFinalizer);
        }
        private TContract CreateOriginContract(Transporter light)
        {
            var memebers = ProxyContractFactory.ParseContractInterface(typeof(TContract));
            var dispatcher = _contractBuilder.ReceiveDispatcherFactory();
            var inputMessages = memebers.GetMethods().Select(m => new MessageTypeInfo
            {
                ReturnType = m.Value.ReturnType,
                ArgumentTypes = m.Value.GetParameters().Select(p => p.ParameterType).ToArray(),
                MessageId = (short)m.Key
            });

            var outputMessages = memebers.GetProperties().Select(m => new MessageTypeInfo
            {
                ArgumentTypes = ReflectionHelper.GetDelegateInfoOrNull(m.Value.PropertyType).ParameterTypes,
                ReturnType = ReflectionHelper.GetDelegateInfoOrNull(m.Value.PropertyType).ReturnType,
                MessageId = (short)m.Key
            });

            var messenger = new Messenger(
                light,
                SerializerFactory.CreateDefault(_contractBuilder.UserSerializationRules.ToArray()),
                DeserializerFactory.CreateDefault(_contractBuilder.UserDeserializationRules.ToArray()),
                outputMessages: outputMessages.ToArray(),
                inputMessages: inputMessages.ToArray()
            );

            var interlocutor = new Interlocutor(messenger, dispatcher);

            TContract contract = _contractBuilder.OriginContractFactory(light.Channel);
            OriginContractLinker.Link(contract, interlocutor);
            return contract;
        }
        private TContract CreateProxyContract(Transporter light)
        {
            var memebers = ProxyContractFactory.ParseContractInterface(typeof(TContract));
            var dispatcher = _contractBuilder.ReceiveDispatcherFactory();

            var outputMessages = memebers.GetMethods().Select(m => new MessageTypeInfo
            {
                ReturnType = m.Value.ReturnType,
                ArgumentTypes = m.Value.GetParameters().Select(p => p.ParameterType).ToArray(),
                MessageId = (short)m.Key
            });

            var inputMessages = memebers.GetProperties().Select(m => new MessageTypeInfo
            {
                ArgumentTypes = ReflectionHelper.GetDelegateInfoOrNull(m.Value.PropertyType).ParameterTypes,
                ReturnType = ReflectionHelper.GetDelegateInfoOrNull(m.Value.PropertyType).ReturnType,
                MessageId = (short)m.Key
            });

            var messenger = new Messenger(
                light,
                SerializerFactory.CreateDefault(_contractBuilder.UserSerializationRules.ToArray()),
                DeserializerFactory.CreateDefault(_contractBuilder.UserDeserializationRules.ToArray()),
                outputMessages: outputMessages.ToArray(),
                inputMessages: inputMessages.ToArray()
            );

            var interlocutor = new Interlocutor(messenger, dispatcher);

            var contract = ProxyContractFactory.CreateProxyContract<TContract>(interlocutor);
            return contract;
        }

        public TContract BuidStateLess()
        {
            throw new NotImplementedException();
        }
    }
}