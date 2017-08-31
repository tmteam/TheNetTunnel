using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Example.Stage1_EasyStart;
using Example.Stage2Example;
using Example.Stage2_ComplexExample;
using Example.Stage3_IntroducingToTestingExample;
using TNT.Api;
using TNT.Contract;
using TNT.Tcp;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TNT examples application. Choose an example:");

            Console.WriteLine("1. Easy start chat example");
            Console.WriteLine("2. Complex chat example");

            Console.WriteLine("3. Integration test example");
            while (true)
            {
                Console.Write("Enter your choice: ");
                var choice = Console.ReadKey();
                Console.WriteLine();
                Console.WriteLine();

                switch (choice.Key)
                {
                    case ConsoleKey.D1:
                    {
                        new Stage1_EasyStartExample().Run();
                            return;
                    }
                    case ConsoleKey.D2:
                        {
                            new Stage2_EasyStartExample().Run();
                            return;
                        }
                    case ConsoleKey.D3:
                        {
                            new Stage3_Example().Run();
                            return;
                        }
                    case ConsoleKey.Escape:
                    {
                        return;
                    }
                }
            }
        }
    }

  
}
