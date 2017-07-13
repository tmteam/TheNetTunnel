using System;

namespace Expirements.General
{
    public class TestContractImplementation : ITestContract
    {
        public string SendChatMessage(DateTime time, string clientName, string message)
        {
           // Console.WriteLine($"[{time}] {clientName}: {message}");
            return $"received {message}";
        }
    }
}