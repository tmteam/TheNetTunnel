namespace TNT.Tests.Contracts
{
    public class CallContract : ICallContract
    {
        public const int SayVoidId = 123;
        public const int SayIntStringId = 321;
        public const int AskVoidId = 111;
        public const int AskSummId = 222;

        public const double AskVoidReturns = 328.0;

        public bool SayVoidCalled = false;
        public bool SayIntStringCalled { get; set; } = false;
        public int SayIntArg { get; set; } = 0;
        public string SayStringArg { get; set; } = null;
        public void SayVoid()
        {
            SayVoidCalled = true;
        }
     
        public void SayIntString(int arg1, string arg2)
        {
            SayIntArg = arg1;
            SayStringArg = arg2;
            SayIntStringCalled = true;
        }

        public double AskVoid()
        {
            return AskVoidReturns;
        }

        public double AskSumm(double a, double b)
        {
            return a + b;
        }
    }
}
