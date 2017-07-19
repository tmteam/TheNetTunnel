using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Exceptions
{
    public class NetworkDeadLockException: Exception
    {
        public NetworkDeadLockException(string message = null)
            : base(message ?? "Remote procedure call deadlock detected. Change receive dispatcher to avoid ")
        {
            
        }
    }
}
