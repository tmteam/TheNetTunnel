using System;

namespace TNT.Tests.Contracts
{
    public class ExceptionalContract : IExceptionalContract
    {
        public void Say()
        {
            throw new InvalidOperationException();
        }

        public int Ask()
        {
            throw new InvalidOperationException();
        }
    }
}
