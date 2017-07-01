using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TNT.Tools;

namespace Testing
{
    public class Test_EnumerStreams
    {
        public void ReadonlyStreamOfFixedSizeEnumeration()
        {
            const int size = 10000;
            var list = new List<byte>(Enumerable.Range(0, size).Select(a => (byte) (a%256)));
            using (var originStream = new MemoryStream(list.ToArray())) {
                originStream.Position = 0;
                using (var str = new ReadonlyStreamOfFixedSizeEnumeration(list)) {

                    str.Position = 0;
                    var buff = new byte[4096];
                    str.Read(buff, 0, 4096);
                    
                    if (buff.Where((t, i) => list[i] != t).Any())
                        throw new Exception();

                    originStream.Read(buff, 0, 4096);
                    if (str.Position != originStream.Position)
                        throw new Exception();

                    str.Position = 5123;

                    str.Read(buff, 0, 4096);

                    if (buff.Where((t, i) => list[i + 5123] != t).Any())
                        throw new Exception();

                    originStream.Position = 5123;
                    originStream.Read(buff, 0, 4096);

                    if (buff.Where((t, i) => list[i + 5123] != t).Any())
                        throw new Exception();

                    if (str.Position != originStream.Position)
                        throw new Exception();
                    
                }
            }
        }

        public void StreamOfEnumeration()
        {
            const int size = 10000;
            var list = new List<byte>(Enumerable.Range(0, size).Select(a => (byte)(a % 256)));
            List<byte> resultList = null;
            using (var originStream = new MemoryStream(list.ToArray()))
            {
                originStream.Position = 0;
                using (var str = new StreamOfEnumeration(list))
                {

                    str.Position = 0;
                    var buff = new byte[4096];
                    str.Read(buff, 0, 4096);

                    if (buff.Where((t, i) => list[i] != t).Any())
                        throw new Exception();

                    originStream.Read(buff, 0, 4096);
                    if (str.Position != originStream.Position)
                        throw new Exception();

                    str.Position = 5123;

                    str.Read(buff, 0, 4096);

                    if (buff.Where((t, i) => list[i + 5123] != t).Any())
                        throw new Exception();

                    originStream.Position = 5123;
                    originStream.Read(buff, 0, 4096);

                    if (str.Position != originStream.Position)
                        throw new Exception();
                    resultList = str.GetList();
                }
            }
            if (resultList == list)
                throw new Exception();
            if (resultList.Where((v, i) => list[i] != v).Any())
                throw new Exception();
        }
    }
}
