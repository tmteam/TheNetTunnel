using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TNT.Api;
using TNT.Contract;
using TNT.Tcp;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            new EasyStartExample().Run();
            Console.WriteLine("Press any key for exit...");
            Console.ReadKey();
        }
    }

  
}
