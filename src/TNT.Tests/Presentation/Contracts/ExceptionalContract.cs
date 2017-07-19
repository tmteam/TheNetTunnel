using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Tests.Presentation.Contracts
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
