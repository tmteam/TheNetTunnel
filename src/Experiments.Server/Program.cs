using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Expirements.General;
using TNT.Channel;
using TNT.Cord;
using TNT.Cord.Deserializers;
using TNT.Cord.Serializers;
using TNT.Light;
using TNT.Light.Sending;
using TNT.Presentation;
using TNT.Presentation.Origin;
using TNT.Presentation.Proxy;

namespace Experiments.Server
{
    class Program
    {
        static TcpListener listener;
        static LightChannel channel;
        static CordMessenger messenger;

        static void Main(string[] args)
        {

            listener = new TcpListener(17171);
            Console.WriteLine("Start listen");

            listener.Start();
            Console.WriteLine("Listen started");

            var client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected");

            var dispatcher = new ConveyorDispatcher();
            var sendSeparationBehaviour = new FIFOSendMessageSequenceBehaviour();
            channel = new LightChannel(
                underlyingChannel: new TcpChannel(client),
                sendMessageSequenceBehaviour: new FIFOSendMessageSequenceBehaviour(),
                receiveMessageThreadBehavior: dispatcher);

            channel.OnReceive += Channel_OnReceive;
            channel.OnDisconnect += ChannelOnOnDisconnect;


            messenger = new CordMessenger(
                channel,
                SerializerFactory.CreateDefault(),
                DeserializerFactory.CreateDefault(),
                outputMessages: new MessageTypeInfo[0],
                inputMessages: new[]
                {
                    new MessageTypeInfo
                    {
                        ReturnType = typeof(string),
                        ArgumentTypes = new[] { typeof(DateTime), typeof(string), typeof(string)},
                        messageId = 42,
                    }
                }
            );
            var interlocutor = new CordInterlocutor(messenger);
            var contract = new TestContractImplementation();
            OriginContractLinker.Link<ITestContract>(contract, interlocutor);

            //messenger.OnAsk+= MessengerOnOnAsk;
            channel.AllowReceive = true;
            while (true)
            {
                string msg;

                msg = Console.ReadLine();
                if (msg == "exit")
                {
                    return;
                }
                using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(msg)))
                {
                    channel.Write(stream);
                }
            }
        }

        private static void MessengerOnOnAsk(ICordMessenger cordMessenger, int msgId, int askId, object[] arguments)
        {
            //Console.Write($"received: {msgId} askId: {askId}");
            //foreach (var o in arguments)
            //{
            //    Console.Write(o.ToString() + " ");
            //}
            //Console.WriteLine();
            messenger.Ans((short )-msgId, (short)askId, "MySuperAnswer");
        }

       
        private static void ChannelOnOnDisconnect(LightChannel lightChannel)
        {
            Console.WriteLine("Disconnected!!");
        }

        private static void Channel_OnReceive(LightChannel arg1, System.IO.MemoryStream arg2)
        {
            //var array = arg2.ToArray();
            //Console.WriteLine(BitConverter.ToString(array));
        }

    }

   

    
}
