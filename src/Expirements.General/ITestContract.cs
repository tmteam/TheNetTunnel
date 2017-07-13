using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNT.Presentation.Proxy;

namespace Expirements.General
{
    public interface ITestContract
    {
        [ContractMessage(42)]
        string SendChatMessage(DateTime time, string clientName, string message);
    }
}
