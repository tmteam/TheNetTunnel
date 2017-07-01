using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ManualResetEvent onpong = new ManualResetEvent(false);

            var contract = new ClientContract();
            contract.ReceivePong += (DateTime time, int i) =>
                {
                    Console.WriteLine("GotAPong" + i);
                    onpong.Set();
                };

            TheTunnel.LightTunnelClient<ClientContract> client = null;

            while (true)
            {
                Console.WriteLine("Enter an ip:");
                var ip = Console.ReadLine();
                var port = "4242";

                client = new TheTunnel.LightTunnelClient<ClientContract>();
                client.OnDisconnect += (object sender, TheTunnel.DisconnectReason reason) =>{
                    Console.WriteLine("Server Connection is closed. Reason: " + reason);
                };

                try
                {
                    client.Connect(IPAddress.Parse(ip), int.Parse(port), contract);
                    break;
                }
                catch
                {
                    Console.WriteLine("Cannot connect to " + ip);
                    Console.WriteLine("try again? [y]");
                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        Console.WriteLine("Bye");
                        return;
                    }
                }
            }
            Console.WriteLine("Succesfully connected");
            Console.WriteLine("PingPong1");
            Stopwatch swGlob = new Stopwatch();
            swGlob.Start();
            for(int i = 0 ; i<1000; i++)
            {
               
                Stopwatch sw = new Stopwatch();
                    sw.Start();
                var res = contract.Ask(i);
               
                sw.Stop();
                Console.WriteLine("i = " + i + " Elasped: " + sw.ElapsedMilliseconds);
            }
            Console.WriteLine("PingPong2");
            
            for (int i = 0; i < 1000; i++){
                var n = DateTime.Now;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Console.WriteLine("SendAPing " + i);
                contract.SendPing(n, i);
                if (!onpong.WaitOne(5000))
                {
                    Console.WriteLine("Fail");
                    return;
                }
                sw.Stop();
                Console.WriteLine("i = " + i + " Elasped: " + sw.ElapsedMilliseconds);
                onpong.Reset();
            }
            swGlob.Stop();
            Console.WriteLine("All is done in " + swGlob.ElapsedMilliseconds);
            Console.WriteLine("PAK2C");
            Console.ReadKey();
        }
    }
}
