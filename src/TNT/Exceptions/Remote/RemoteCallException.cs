using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Exceptions
{
    public abstract class RemoteCallException: Exception
    {
        protected RemoteCallException(
            bool isFatal, 
            RemoteCallExceptionId id, 
            string message = null, 
            Exception innerException = null)
            :base($"[{id}]"+ (message??(" tnt call exception")), innerException)
        {
            IsFatal = isFatal;
            Id = id;
        }
        public bool IsFatal { get;}
        public RemoteCallExceptionId Id { get; }

        public short? CordId { get; }
        public short? AskId { get; }

        public static RemoteCallException Create(RemoteCallExceptionId type, string additionalInfo, short? cordId,
            short? askId)
        {
            switch (type)
            {
                case RemoteCallExceptionId.ConnectionIsLostException:
                    return new ConnectionIsLostException(additionalInfo);
                case RemoteCallExceptionId.ConnectionIsNotEstablishedYet:
                    return new ConnectionIsNotEstablishedYet(additionalInfo);
                case RemoteCallExceptionId.RemoteSideUnhandledException:
                    return new RemoteSideUnhandledException(cordId,askId, additionalInfo);
                case RemoteCallExceptionId.LocalSideSerializationException:
                    return  new LocalSideSerializationException(additionalInfo);
                case RemoteCallExceptionId.RemoteSideSerializationException:
                    return new RemoteSideSerializationException(cordId.Value, askId, additionalInfo);
                case RemoteCallExceptionId.RemoteContractImplementationException:
                    return new RemoteContractImplementationException(cordId.Value, additionalInfo);
                default:
                    throw new InvalidOperationException(
                        $"Exception type {type} is unknown. Exception message: {additionalInfo}"); 
            }
        }
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
