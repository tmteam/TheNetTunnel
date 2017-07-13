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
        string Ask(DateTime time, string clientName, string message);
        [ContractMessage(43)]
        void Say(DateTime time, string clientName, string message);
        [ContractMessage(44)]
        Action<string> SayCallBack { get; set; }
        [ContractMessage(45)]
        Func<string,int> AskCallBack { get; set; }
    }
}
