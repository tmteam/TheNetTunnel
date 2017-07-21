using System;
using TNT.Exceptions;

namespace TNT.Cord
{
    public class ExceptionMessage
    {
        //public static ExceptionMessage CreateForUnhandledDuringCall(Exception unhandledException)
        //{
            
        //}
        public static ExceptionMessage CreateBy(Exception rcExccException)
        {
            throw new NotImplementedException();
        }
        public short CordId { get; set; }
        public short AskId { get; set; }
        public RemoteCallExceptionId ExceptionType { get; set; }
        public string AdditionalExceptionInformation { get; set; }
        public  RemoteCallException Exception { get; }
    }
}