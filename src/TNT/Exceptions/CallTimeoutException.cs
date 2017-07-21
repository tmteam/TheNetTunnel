using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Exceptions
{
    public class CallTimeoutException: Exception
    {
        public CallTimeoutException(int cordId, int askId)
        {
            
        }
    }
}
