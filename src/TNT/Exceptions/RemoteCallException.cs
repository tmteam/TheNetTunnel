using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Exceptions
{
    public abstract class RemoteCallException: Exception
    {
        protected RemoteCallException(bool connectionIsLost, RemoteCallExceptionId id, string message = null)
            :base($"[{id}]"+ (message??(" tnt call exception")))
        {
            IsConnectionLost = connectionIsLost;
            Id = id;
        }
        public bool IsConnectionLost { get;}
        public RemoteCallExceptionId Id { get; }
    }


    public enum RemoteCallExceptionId
    {
        ConnectionIsLostException = 1,
        ConnectionIsNotEstablishedYet =2,
        RemoteSideUnhandledException = 3,
        LocalSideSerializationException = 4,
        RemoteSideSerializationException = 5,
        RemoteContractImplementationException = 6,
    }
}
