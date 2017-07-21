using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Exceptions
{
    public interface ITntCallException
    {
        bool IsFatal { get; }
        short? CordId { get; }
        short? AskId { get; }
    }
}
