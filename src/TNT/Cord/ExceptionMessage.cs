using System;
using TNT.Exceptions;

namespace TNT.Cord
{
    public class ExceptionMessage
    {
        public short CordId { get; set; }
        public short AskId { get; set; }
        public RemoteCallExceptionId ExceptionType { get; set; }
        public string AdditionalExceptionInformation { get; set; }

        public RemoteCallException ToException()
        {
            throw new NotImplementedException();
        }
    }
}