using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TNT.Channel;
using TNT.Cord;
using TNT.Cord.Deserializers;
using TNT.Cord.Serializers;
using TNT.Light;
using TNT.Light.Sending;
using TNT.Presentation;
using TNT.Presentation.Origin;
using TNT.Presentation.Proxy;

namespace Expirements.General
{
    public static class Factory
    {
        public static TContract CreateForClient<TContract>(TcpClient client)
        {
            var dispatcher = new ConveyorDispatcher();
            var sendSeparationBehaviour = new FIFOSendMessageSequenceBehaviour();
            var channel = new LightChannel(
                underlyingChannel: new TcpChannel(client),
                sendMessageSequenceBehaviour: new FIFOSendMessageSequenceBehaviour(),
                receiveMessageThreadBehavior: dispatcher);

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
                 channel,
                 SerializerFactory.CreateDefault(),
                 DeserializerFactory.CreateDefault(),
                 outputMessages: outputMessages.ToArray(),
                 inputMessages: inputMessages.ToArray()
             );
            var interlocutor = new CordInterlocutor(messenger);
            var contract = ProxyContractFactory.CreateProxyContract<TContract>(interlocutor);
            channel.AllowReceive = true;
            return contract;
        }
        public static ITestContract CreateForServer(TcpClient client)
        {
            var dispatcher = new ConveyorDispatcher();
            var sendSeparationBehaviour = new FIFOSendMessageSequenceBehaviour();
            var channel = new LightChannel(
                underlyingChannel: new TcpChannel(client),
                sendMessageSequenceBehaviour: new FIFOSendMessageSequenceBehaviour(),
                receiveMessageThreadBehavior: dispatcher);

            var memebers = ProxyContractFactory.ParseContractInterface(typeof(ITestContract));

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
                channel,
                SerializerFactory.CreateDefault(),
                DeserializerFactory.CreateDefault(),
                outputMessages: outputMessages.ToArray(),
                inputMessages: inputMessages.ToArray()
            );
            var interlocutor = new CordInterlocutor(messenger);
            var contract = new TestContractImplementation();
            var handlers = OriginContractLinker.Link<ITestContract>(contract, interlocutor);
            channel.AllowReceive = true;
            return contract;
        }
    }
}
