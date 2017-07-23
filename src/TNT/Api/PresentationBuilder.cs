using System;
using System.Collections.Generic;
using TNT.Presentation.Deserializers;
using TNT.Presentation.ReceiveDispatching;
using TNT.Presentation.Serializers;
using TNT.Transport;
using TNT.Transport.Sending;

namespace TNT.Api
{
    public class PresentationBuilder<TContract> where TContract:class
    {
        public Func<ISendPduBehaviour> SendMessageSequenceBehaviourFactory { get; private set; } = ()=> new FIFOSendPduBehaviour();

        public Func<IDispatcher> ReceiveDispatcherFactory { get; private set; } = ()=>new ConveyorDispatcher();

        public Action<TContract, IChannel> ContractInitializer { get; private set; } = (contract, channel) => { };
        public Action<TContract, IChannel> ContractFinalizer   { get; private set; } = (contract, channel) => { };

        
        public List<DeserializationRule> UserDeserializationRules { get; } = new List<DeserializationRule>();

        public List<SerializationRule> UserSerializationRules { get; } = new List<SerializationRule>();

        public Type ContractInterfaceType { get; } = typeof(TContract);

        public Func<IChannel, TContract> OriginContractFactory { get; }

        internal PresentationBuilder()
        {
            OriginContractFactory = null;
        }
        internal PresentationBuilder(Func<IChannel, TContract> contractFactory)
        {
            if(contractFactory==null)
                throw new ArgumentNullException(nameof(contractFactory));
            OriginContractFactory = contractFactory;
        }

        

        public PresentationBuilder<TContract> UseSendSeparationBehaviour<TSendMessageSequenceBehaviour>() 
            where TSendMessageSequenceBehaviour: ISendPduBehaviour, new()
        {
            return UseSendSeparationBehaviour(()=>new TSendMessageSequenceBehaviour());
        }
        public PresentationBuilder<TContract> UseSendSeparationBehaviour(Func<ISendPduBehaviour> sendMessageSequenceBehaviourFactory)
        {
            if (sendMessageSequenceBehaviourFactory == null)
                throw new ArgumentNullException(nameof(sendMessageSequenceBehaviourFactory));
            SendMessageSequenceBehaviourFactory = sendMessageSequenceBehaviourFactory;
            return this;
        }
        public PresentationBuilder<TContract> UseReceiveDispatcher<TDispatcher>()
            where TDispatcher : IDispatcher, new()
        {
            return this.UseReceiveDispatcher(() => new TDispatcher());
        }
        public PresentationBuilder<TContract> UseReceiveDispatcher(Func<IDispatcher> dispatcherFactory)
        {
            if (dispatcherFactory == null)
                throw new ArgumentNullException(nameof(dispatcherFactory));
            ReceiveDispatcherFactory = dispatcherFactory;
            return this;
        }


        public PresentationBuilder<TContract> UseSerializer(SerializationRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));
            UserSerializationRules.Add(rule);
            return this;
        }

        public PresentationBuilder<TContract> UseSerializer<TType, TSerializer>() where TSerializer: ISerializer, new ()
        {
            return UseSerializer(new SerializationRule((t)=> t== typeof(TType), (t,f)=>new TSerializer()));
        }

        public PresentationBuilder<TContract> UseDeserializer(DeserializationRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            UserDeserializationRules.Add(rule);
            return this;
        }

        public PresentationBuilder<TContract> UseDeserializer<TType, TDeserializer>() where TDeserializer: IDeserializer, new()
        {

            return UseDeserializer(new DeserializationRule((t) => t == typeof(TType), (t) => new TDeserializer()));
        }

        public PresentationBuilder<TContract> UseContractInitalization(Action<TContract, IChannel> initializer)
        {
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            ContractInitializer = initializer;
            return this;
        }
        public PresentationBuilder<TContract> UseContractFinalization(Action<TContract, IChannel> finalizer)
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

    }
}