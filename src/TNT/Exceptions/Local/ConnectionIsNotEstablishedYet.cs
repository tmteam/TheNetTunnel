using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Exceptions
{
    public class ConnectionIsNotEstablishedYet: RemoteCallException
    {
        public ConnectionIsNotEstablishedYet(string message = null) : base(true, RemoteCallExceptionId.ConnectionIsNotEstablishedYet, message)
        {
        }
    }
}
