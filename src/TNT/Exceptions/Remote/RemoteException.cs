using System;
using TNT.Exceptions.ContractImplementation;
using TNT.Exceptions.Local;

namespace TNT.Exceptions.Remote
{
    /// <summary>
    /// Remote error occurred during call processing
    /// like serialization, 
    /// wrong contractImplementation, 
    /// user code exceptions etc...
    /// </summary>
    public abstract class RemoteException: TntCallException
    {
        public const string DefaultExceptionText = "tnt call exception without any additional information";
        protected RemoteException(
            ErrorType id,
            bool isFatal,
            short? messageId,
            short? askId,
            string message, 
            Exception innerException = null)
            :base(isFatal, messageId, askId, 
                 $"[{id}]"+ (message?? DefaultExceptionText), 
                 innerException)
        {
            Id = id;
        }

        public ErrorType Id { get; }

        public static RemoteException Create(ErrorType type, string additionalInfo, short? messageId,
            short? askId, bool isFatal = false)
        {
            switch (type)
            {
              case ErrorType.UnhandledUserExceptionError:
                    return new RemoteUnhandledException(messageId,askId, null, additionalInfo);
                case ErrorType.SerializationError:
                    return new RemoteSerializationException(messageId, askId, isFatal, additionalInfo);
                case ErrorType.ContractSignatureError:
                    return new RemoteContractImplementationException(messageId, askId, isFatal, additionalInfo);
                default:
                    throw new InvalidOperationException(
                        $"Exception type {type} is unknown. Exception message: {additionalInfo}"); 
            }
        }
    }
}