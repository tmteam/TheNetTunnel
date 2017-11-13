using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace TNT.Tests.Serialization
{
    public static class SerializeTestHelper
    {
       public static T SerializeAndBack<TSerializer, TDeserializer, T>(T value)
       where TSerializer : ISerializer<T>, new()
       where TDeserializer : IDeserializer<T>, new()
        {
            using (var result = new MemoryStream())
            {
                var serializer = new TSerializer();
                serializer.SerializeT(value, result);

                result.Position = 0;
                return new TDeserializer().DeserializeT(result, (int)result.Length);
            }
        }
    }
}
