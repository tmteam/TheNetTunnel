using System;

namespace Expirements.General
{
    public class TestContractImplementation : ITestContract
    {
        public string Ask(DateTime time, string clientName, string message)
        {
            Console.WriteLine($"[{time}] {clientName}: {message}");
            return $"received {message}";
        }

        public void Say(int id)
        {
           // Console.WriteLine("Received: "+ message);
            SayCallBack(id);
        }

        public Action<int> SayCallBack { get; set; }
        public Func<string, int> AskCallBack { get; set; }
    }
}