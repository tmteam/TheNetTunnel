using System;
using System.Net.Sockets;
using System.Threading;
using TNT.Channel;
using TNT.Cord;
using TNT.Cord.Deserializers;
using TNT.Cord.Serializers;
using TNT.Light;
using TNT.Light.Sending;
using TNT.Presentation;

namespace Expirements
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Experiments client");
            ConveyorDispatcher dispatcher = new ConveyorDispatcher();
            var sendSeparationBehaviour = new FIFOSendMessageSequenceBehaviour();
            TcpClient client = new TcpClient();
            var tcpChannel = new TcpChannel(client);
            var channel = new LightChannel(tcpChannel, sendSeparationBehaviour, dispatcher);
            channel.OnDisconnect+= ChannelOnOnDisconnect;

            Thread.Sleep(1000);
            client.Connect("127.0.0.1", 17171);
            

            var messenger = new CordMessenger(
                channel,
                SerializerFactory.CreateDefault(),
                DeserializerFactory.CreateDefault(),
                outputMessages: new[]
                {
                    new MessageTypeInfo
                    {
                        ArgumentTypes = new[] {typeof(DateTime), typeof(string), typeof(string)},
                        messageId = 42,
                        ReturnType = typeof(string)
                    }
                },
                inputMessages: new MessageTypeInfo[0]
            );

            var interlocutor = new CordInterlocutor(messenger);

            //messenger.OnAns += Messenger_OnAns;
            channel.AllowReceive = true;

            while (true)
            {
                string msg;

                msg = Console.ReadLine();
                if (msg == "exit")
                {
                    return;
                }

                //messenger.Say(42, new object[]{DateTime.Now, "Client", msg});
                // messenger.Ask(42, 115, new object[] { DateTime.Now, "Client", msg });
                var ans = interlocutor.Ask<string>(42, new object[] {DateTime.Now, "Client", msg});

                Console.WriteLine("Answer: "+ ans);
            }


        }

        private static void Messenger_OnAns(ICordMessenger arg1, int msgId, int askId, object result)
        {
            Console.WriteLine($"Answer msgId={msgId} askId={askId} result = {result}");

        }

        private static void ChannelOnOnDisconnect(LightChannel lightChannel)
        {
            Console.WriteLine("Disconnected!!");
        }


     
    }
}
