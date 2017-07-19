using System;
using System.Collections.Generic;
using System.Linq;
using TNT.Channel;
using TNT.Channel.Tcp;
using TNT.Cord;
using TNT.Cord.Deserializers;
using TNT.Cord.Serializers;
using TNT.Light;
using TNT.Light.Sending;
using TNT.Presentation.Origin;
using TNT.Presentation.Proxy;

namespace TNT.Presentation
{
    public static class ConnectionBuilder
    {
        public static ConnectionBuilder<TContract> UseContract<TContract>()
            where TContract : class
        {
             return new ConnectionBuilder<TContract>();
        }

        public static ConnectionBuilder<TContract> UseContract<TContract, TImplementation>() 
            where TContract: class 
            where TImplementation: TContract, new()
        {
            return UseContract<TContract>((c) => new TImplementation());
        }
        public static ConnectionBuilder<TContract> UseContract<TContract>(TContract implementation)
            where TContract : class
        {
            return UseContract<TContract>((c) => implementation);
        }
        public static ConnectionBuilder<TContract> UseContract<TContract>(Func<TContract> implementationFactory)
            where TContract : class
        {
            return UseContract<TContract>((c) => implementationFactory());
        }

        public static ConnectionBuilder<TContract> UseContract<TContract>(Func<IChannel, TContract> implementationFactory)
            where TContract : class
        {
            return new ConnectionBuilder<TContract>(implementationFactory);
        }
    }

    public class ConnectionBuilder<TContract> where TContract:class
    {
        public Func<ISendMessageSequenceBehaviour> SendMessageSequenceBehaviourFactory { get; private set; } = ()=> new FIFOSendMessageSequenceBehaviour();

        public Func<IDispatcher> ReceiveDispatcherFactory { get; private set; } = ()=>new ConveyorDispatcher();

        public Action<TContract, IChannel> ContractInitializer { get; private set; } = (contract, channel) => { };
        public Action<TContract, IChannel> ContractFinalizer   { get; private set; } = (contract, channel) => { };

        
        public List<DeserializationRule> UserDeserializationRules { get; } = new List<DeserializationRule>();

        public List<SerializationRule> UserSerializationRules { get; } = new List<SerializationRule>();

        public Type ContractInterfaceType { get; } = typeof(TContract);

        public Func<IChannel, TContract> OriginContractFactory { get; }

        internal ConnectionBuilder()
        {
            OriginContractFactory = null;
        }
        internal ConnectionBuilder(Func<IChannel, TContract> contractFactory)
        {
            if(contractFactory==null)
                throw new ArgumentNullException(nameof(contractFactory));
            OriginContractFactory = contractFactory;
        }

        

        public ConnectionBuilder<TContract> UseSendSeparationBehaviour<TSendMessageSequenceBehaviour>() 
            where TSendMessageSequenceBehaviour: ISendMessageSequenceBehaviour, new()
        {
            return UseSendSeparationBehaviour(()=>new TSendMessageSequenceBehaviour());
        }
        public ConnectionBuilder<TContract> UseSendSeparationBehaviour(Func<ISendMessageSequenceBehaviour> sendMessageSequenceBehaviourFactory)
        {
            if (sendMessageSequenceBehaviourFactory == null)
                throw new ArgumentNullException(nameof(sendMessageSequenceBehaviourFactory));
            SendMessageSequenceBehaviourFactory = sendMessageSequenceBehaviourFactory;
            return this;
        }
        public ConnectionBuilder<TContract> UseReceiveDispatcher<TDispatcher>()
            where TDispatcher : IDispatcher, new()
        {
            return this.UseReceiveDispatcher(() => new TDispatcher());
        }
        public ConnectionBuilder<TContract> UseReceiveDispatcher(Func<IDispatcher> dispatcherFactory)
        {
            if (dispatcherFactory == null)
                throw new ArgumentNullException(nameof(dispatcherFactory));
            ReceiveDispatcherFactory = dispatcherFactory;
            return this;
        }

        public ConnectionBuilder<TContract> DontUseDefaultSerializers()
        {
            throw new NotImplementedException();
        }

        public ConnectionBuilder<TContract> DontUseDefaultDeserializers()
        {
            throw new NotImplementedException();
        }

        public ConnectionBuilder<TContract> UseSerializer(SerializationRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));
            UserSerializationRules.Add(rule);
            return this;
        }

        public ConnectionBuilder<TContract> UseSerializer<TType, TSerializer>() where TSerializer: ISerializer, new ()
        {
            return UseSerializer(new SerializationRule((t)=> t== typeof(TType), (t,f)=>new TSerializer()));
        }

        public ConnectionBuilder<TContract> UseDeserializer(DeserializationRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            UserDeserializationRules.Add(rule);
            return this;
        }

        public ConnectionBuilder<TContract> UseDeserializer<TType, TDeserializer>() where TDeserializer: IDeserializer, new()
        {

            return UseDeserializer(new DeserializationRule((t) => t == typeof(TType), (t) => new TDeserializer()));
        }

        public ConnectionBuilder<TContract> UseContractInitalization(Action<TContract, IChannel> initializer)
        {
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            ContractInitializer = initializer;
            return this;
        }
        public ConnectionBuilder<TContract> UseContractFinalization(Action<TContract, IChannel> finalizer)
        {
            if(finalizer == null)
                throw  new ArgumentNullException(nameof(finalizer));
            ContractFinalizer = finalizer;
            return this;
        }

        public ConnectionBuilder<TContract, TChannel> UseChannel<TChannel>() 
            where TChannel: IChannel, new()
        {
            return UseChannel<TChannel>(() => new TChannel());
        }
        
        public ConnectionBuilder<TContract, TChannel> UseChannel<TChannel>(TChannel theChannel) 
            where TChannel : IChannel
        {
            if (theChannel == null)
                throw new ArgumentNullException(nameof(theChannel));
            return UseChannel<TChannel>(() => theChannel);
        }

        public ConnectionBuilder<TContract, TChannel> UseChannel<TChannel>(Func<TChannel> channelFactory) 
            where TChannel : IChannel
        {
            if (channelFactory == null)
                throw new ArgumentNullException(nameof(channelFactory));
            return new ConnectionBuilder<TContract, TChannel>(this, channelFactory);
        }

        public object UseContractInitalization()
        {
            throw new NotImplementedException();
        }
    }

    public class ConnectionBuilder<TContract, TChannel> 
            where TChannel : IChannel
            where TContract: class 
    {
        private readonly ConnectionBuilder<TContract> _contractBuilder;
        private readonly Func<TChannel> _channelFactory;

        public  ConnectionBuilder(ConnectionBuilder<TContract> contractBuilder, Func<TChannel> channelFactory)
        {
            _contractBuilder = contractBuilder;
            _channelFactory = channelFactory;
        }
        public Connection<TContract, TChannel> Buid()
        {
            var dispatcher = _contractBuilder.ReceiveDispatcherFactory();
            var sendSeparationBehaviour = _contractBuilder.SendMessageSequenceBehaviourFactory();
            var channel = _channelFactory();

            var light = new LightChannel(
                underlyingChannel: channel,
                sendMessageSequenceBehaviour: sendSeparationBehaviour,
                receiveMessageThreadBehavior: dispatcher);

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
        private TContract CreateOriginContract(LightChannel light)
        {
            var memebers = ProxyContractFactory.ParseContractInterface(typeof(TContract));

            var inputMessages = memebers.GetMethods().Select(m => new MessageTypeInfo
            {
                ReturnType = m.Value.ReturnType,
                ArgumentTypes = m.Value.GetParameters().Select(p => p.ParameterType).ToArray(),
                messageId = (short)m.Key
            });

            var outputMessages = memebers.GetProperties().Select(m => new MessageTypeInfo
            {
                ArgumentTypes = ReflectionHelper.GetDelegateInfoOrNull(m.Value.PropertyType).ParameterTypes,
                ReturnType = ReflectionHelper.GetDelegateInfoOrNull(m.Value.PropertyType).ReturnType,
                messageId = (short)m.Key
            });

            var messenger = new CordMessenger(
                light,
                SerializerFactory.CreateDefault(_contractBuilder.UserSerializationRules.ToArray()),
                DeserializerFactory.CreateDefault(_contractBuilder.UserDeserializationRules.ToArray()),
                outputMessages: outputMessages.ToArray(),
                inputMessages: inputMessages.ToArray()
            );

            var interlocutor = new CordInterlocutor(messenger);

            TContract contract = _contractBuilder.OriginContractFactory(light.Channel);
            OriginContractLinker.Link(contract, interlocutor);
            return contract;
        }
        private TContract CreateProxyContract(LightChannel light)
        {
            var memebers = ProxyContractFactory.ParseContractInterface(typeof(TContract));

            var outputMessages = memebers.GetMethods().Select(m => new MessageTypeInfo
            {
                ReturnType = m.Value.ReturnType,
                ArgumentTypes = m.Value.GetParameters().Select(p => p.ParameterType).ToArray(),
                messageId = (short)m.Key
            });

            var inputMessages = memebers.GetProperties().Select(m => new MessageTypeInfo
            {
                ArgumentTypes = ReflectionHelper.GetDelegateInfoOrNull(m.Value.PropertyType).ParameterTypes,
                ReturnType = ReflectionHelper.GetDelegateInfoOrNull(m.Value.PropertyType).ReturnType,
                messageId = (short)m.Key
            });

            var messenger = new CordMessenger(
                light,
                SerializerFactory.CreateDefault(_contractBuilder.UserSerializationRules.ToArray()),
                DeserializerFactory.CreateDefault(_contractBuilder.UserDeserializationRules.ToArray()),
                outputMessages: outputMessages.ToArray(),
                inputMessages: inputMessages.ToArray()
            );

            var interlocutor = new CordInterlocutor(messenger);

            TContract contract;
            contract = ProxyContractFactory.CreateProxyContract<TContract>(interlocutor);
            return contract;
        }

        public TContract BuidStateLess()
        {
            throw new NotImplementedException();
        }
    }
}