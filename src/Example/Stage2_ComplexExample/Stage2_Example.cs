using System;
using System.Net;
using System.Net.Sockets;
using Example.Stage2Example;
using ProtoBuf;
using TNT;
using TNT.Api;
using TNT.Exceptions.Local;
using TNT.Tcp;

namespace Example.Stage2_ComplexExample
{
    public class Stage2_EasyStartExample
    {
        public void Run()
        {
            Console.WriteLine("TNT chat with authotization example.");
            Console.WriteLine("In this example, client authorizes and sends messages in statefull mode");
            Console.WriteLine("Server broadcast received messages to all clients");
            Console.WriteLine("ExceptionHandling is provided");
            Console.WriteLine();

            //All the server code is placed at Server.cs 
            var server = new Server();

            try
            {
                RunClient();
            }
            catch (SocketException e)
            {
                Console.WriteLine("Could not connect to the server");
                throw;
            }
            server.Dispose();
        }

        /// <exception cref="SocketException"></exception>
        static void RunClient()
        {

            Console.WriteLine("Connecting to the server");

          
            using (var client = TntBuilder   // socket exception can be thrown here
                   .UseContract<IStage2Contract>()
                   .CreateTcpClientConnection(IPAddress.Loopback, 12345))
            {
                //subscribing for income message callback:
                client.Contract.NewMessageReceived +=
                    (msg) => Console.WriteLine($"[{msg.Timestamp} {msg.User}] {msg.Message}");

                if (!TryAuthorize(client))
                    return;

                Console.WriteLine("Type your messages or \"exit\" for exit:");
                //Message sending loop:
                while (true)
                {
                    var message = Console.ReadLine();
                    if (message.ToLower() == "exit")
                        break;
                    try
                    {
                        //Trying to send the message
                        var messageId = client.Contract.Send(DateTime.Now, message);
                        Console.WriteLine($"sent with id: {messageId}");
                    }
                    catch (TntCallException e)
                    {
                        if (!client.Channel.IsConnected)
                            Console.WriteLine($"Disconnected because of {e.Message}");
                        break;
                    }
                }
            }
        }

        static bool TryAuthorize(IConnection<IStage2Contract, TcpChannel> client)
        {
            try
            {
                //Authorization:
                if (client.Contract.TryAuthorize("superman", "123"))
                {
                    Console.WriteLine("Authorized succesfully");
                    return true;
                }
                else
                {
                    Console.WriteLine("Authorization failed");
                    return false;
                }
            }
            catch (TntCallException e)
            {
                Console.WriteLine($"Error during authorization: {e.Message}");
                return false;
            }
        }
    }


 
}
