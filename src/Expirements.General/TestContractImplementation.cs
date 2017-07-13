using System;

namespace Expirements.General
{
    public class TestContractImplementation : ITestContract
    {
        public string Ask(DateTime time, string clientName, string message)
        {
           // Console.WriteLine($"[{time}] {clientName}: {message}");
            return $"received {message}";
        }

        public void Say(DateTime time, string clientName, string message)
        {
            Console.WriteLine("Received: "+ message);
           // SayCallBack("Callback for " + message);
        }

        public Action<string> SayCallBack { get; set; }
        public Func<string, int> AskCallBack { get; set; }
    }
}