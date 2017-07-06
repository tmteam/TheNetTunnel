using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TheTunnel;

namespace ServerPong
{
    public class Program
    {
        static LightTunnelServer<ServerContract> server;

        public static void Main(string[] args)
        {
            server = new TheTunnel.LightTunnelServer<ServerContract>();

            server.BeforeConnect += (sender, contract, info) =>{
                contract.ReceivePing += (DateTime dt, int i) => { contract.SendPong(dt, i); };
                contract.Question += (i) => i;
                Console.WriteLine("Client connected  (" + info.Client.Client.Client.RemoteEndPoint.ToString() + ")");
            };

            server.OnDisconnect += (LightTunnelServer<ServerContract> sender, ServerContract contract) =>{
                Console.WriteLine("Client  was disconnected");
            };

            Console.WriteLine("Opening the server");
            server.Open(IPAddress.Any, 4242);
            Console.WriteLine("Waiting of clients");

            while (true)
            {
                var msg = Console.ReadLine();
                if (msg == "exit")
                    return;
            }
            Console.WriteLine("Server closed");
            server.Close();

        }
    }
}
