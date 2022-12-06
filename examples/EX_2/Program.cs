using System;
using EX_2.Stage1_EasyStart;
using EX_2.Stage2_ComplexExample;
using EX_2.Stage3_IntroducingToTestingExample;

namespace EX_2;

class Program
{
    static void Main()
    {
        Console.WriteLine("TNT examples application. Choose an example:");

        Console.WriteLine("1. Easy-start chat example");
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