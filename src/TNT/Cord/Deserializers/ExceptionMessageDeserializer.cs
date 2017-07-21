﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNT.Exceptions;

namespace TNT.Cord.Deserializers
{
    public class ExceptionMessageDeserializer: DeserializerBase<ExceptionMessage>
    {
        private readonly SequenceDeserializer _deserializer;

        public ExceptionMessageDeserializer()
        {
            this.Size = null;
            _deserializer = new SequenceDeserializer(new IDeserializer[]
            {
                  new ValueTypeDeserializer<short>(),
                  new ValueTypeDeserializer<short>(),
                  new EnumDeserializer<RemoteCallExceptionId>(),
                  new UnicodeDeserializer()
            });
        }
        public override ExceptionMessage DeserializeT(Stream stream, int size)
        {
            var deserialized = _deserializer.DeserializeT(stream, size);
            return new ExceptionMessage
            {
                CordId = (short)deserialized[0],
                AskId = (short) deserialized[1],
                ExceptionType = (RemoteCallExceptionId)deserialized[2],
                AdditionalExceptionInformation = (string)deserialized[3]
            };
        }
    }
}