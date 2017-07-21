using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TNT.Exceptions;
using TNT.Exceptions.Remote;

namespace TNT.Cord.Serializers
{
    public class ExceptionMessageSerializer: SerializerBase<ExceptionMessage>
    {
        private  readonly SequenceSerializer _serializer;

        public ExceptionMessageSerializer()
        {
            this.Size = null;
            _serializer = new SequenceSerializer(
                   new ISerializer[]
                   {
                        new ValueTypeSerializer<short>(),
                        new ValueTypeSerializer<short>(),
                        new EnumSerializer<RemoteExceptionId>(),
                        new UnicodeSerializer()
                   });
        }
        public override void SerializeT(ExceptionMessage obj, MemoryStream stream)
        {
             _serializer.SerializeT(
                    new object[] {obj.CordId, obj.AskId, obj.ExceptionType, obj.AdditionalExceptionInformation}, stream);

        }
    }
}
