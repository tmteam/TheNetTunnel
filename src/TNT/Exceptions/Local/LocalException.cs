using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Exceptions.Local
{
    public abstract class LocalException: Exception, ITntCallException
    {
        public LocalException(
            bool isFatal, 
            short? cordId, 
            short? askId, 
            string message,
            Exception innerException  = null)
            :base(message, innerException)
        {
            IsFatal = isFatal;
            CordId = cordId;
            AskId = askId;
        }

        public bool IsFatal { get; }
        public short? CordId { get; }
        public short? AskId { get; }
    }
}
