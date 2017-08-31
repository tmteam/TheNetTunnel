using System;
using System.Net;
using TNT;

namespace Example
{
    public class EasyStartExample
    {
        public void Run()
        {
            Console.WriteLine("TNT easy start example.");
            Console.WriteLine("In this example, client sending messages to the server in stateless mode");
            Console.WriteLine("No exception handling provided");
            Console.WriteLine();

            #region open the server

            Console.WriteLine("Opening the server...");
            var server = TntBuilder
                .UseContract<IChatContract, ServerChatContract>()
                .CreateTcpServer(IPAddress.Any, 12345);
            /*
             * Uncomment folowing strings to see the order of operations:
             
            server.AfterConnect +=
                (_, c) => Console.WriteLine($"[Server] income connection from {c.Channel.RemoteEndpointName}");
            server.Disconnected +=
                (_, c) => Console.WriteLine($"[Server] disconnected {c.Connection.Channel.RemoteEndpointName}");
            */

            Console.WriteLine("Start listening...");
            server.IsListening = true;
            Console.WriteLine("listening is started");

            #endregion

            Console.WriteLine("Type your messages or \"exit\" for exit:");

            while (true)
            {
                var message = Console.ReadLine();
                if (message.ToLower() == "exit")
                    break;
                //Creating new client connection
                using (var client = TntBuilder
                    .UseContract<IChatContract>()
                    .CreateTcpClientConnection(IPAddress.Loopback, 12345))
                {
                   //Sending the message in "fire and foget" style, because the return type is void
                   client.Contract.Send("Superman", message);
                }  //Close connection right after the sending
            }
            server.Close();
        }
    
    }
    /// <summary>
    /// Interface (contract) for client server interaction
    /// </summary>
    public interface IChatContract
    {
        [TntMessage(1)] 
        //Message type number 1. Return type is void so the message sends in "fire and foget" style
        void Send(string user, string message);
    }
    /// <summary>
    /// Server implementation of the interaction contract
    /// </summary>
    public class ServerChatContract : IChatContract
    {
        public ServerChatContract()
        {
            //Server contract creates one time for every connection
            //Console.WriteLine("Server contract is constructed");
        }
        //Message type number 1. Return type is void so the message sends in "fire and foget" style
        public void Send(string user, string message)
        {
            Console.WriteLine($"[Server received:] {user} : {message}");
        }
    }
}
