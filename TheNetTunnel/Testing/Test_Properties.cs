using System;
using System.Collections.Generic;
using System.Linq;
using TNT.Tools.ContractProperties;

namespace Testing
{
    public class Test_Properties
    {
        public void Creation()
        {
            var testList = new List<ITntProperty>();
            
            //ValidTypes:
            
            testList.Add(new TntValueProperty<bool>("a", "b",     false, true));
            testList.Add(new TntValueProperty<ushort>("a", "b",   false, 20));
            testList.Add(new TntValueProperty<uint>("a", "b",     false, 123));
            testList.Add(new TntValueProperty<ulong>("a", "b",    false, 23));
            testList.Add(new TntValueProperty<short>("a", "b",    false, 23));
            testList.Add(new TntValueProperty<int>("a", "b",      false, 23));
            testList.Add(new TntValueProperty<long>("a", "b",     false, 23));
            testList.Add(new TntValueProperty<byte>("a", "b",     false, 23));
            testList.Add(new TntValueProperty<sbyte>("a", "b",    false, 23));
            testList.Add(new TntValueProperty<double>("a", "b",   false, 23));
            testList.Add(new TntValueProperty<float>("a", "b",    false, 23));
            testList.Add(new TntValueProperty<DateTime>("a", "b", false, DateTime.Now));
            testList.Add(new TntValueProperty<string>("a", "b",   false, "123"));

            //InvalidTypes:

            try {
                testList.Add(new TntValueProperty<Test_Properties>("a", "b", true, null));
                throw new Exception();                
            }
            catch (InvalidOperationException ex) {}

            try {
                testList.Add(new TntValueProperty<TimeSpan>("a", "b", true, TimeSpan.FromDays(2)));
                throw new Exception();
            }
            catch (InvalidOperationException ex) { }
        }

        public void Access()
        {
            var tpm = new TntSlavePropertyManager(new ITntProperty[]
            {
                new TntReadonlyProperty<int>("i", "i", 42),
                new TntValueProperty<double>("d", "d", true, 3.14),
                new TntDelegateProperty<int>("id", "id", OnRead, OnWrite),
            });

            var ip = tpm["i"];
            if (ip == null)
                throw new Exception();

            var dp = tpm["m2"];
            if (dp != null)
                throw new Exception();

            if (!tpm["I"].GetObjectValue().Equals(42))
                throw new Exception();

            if (!tpm["id"].GetObjectValue().Equals(33))
                throw new Exception();

            if(!tpm.TrySetValue("D","123"))
                throw new Exception();

            if (tpm.TrySetValue("i", "43"))
                throw new Exception();

            if(tpm.TrySetValue("d","qwe"))
                throw new Exception();

            if(tpm.TrySetValue("qwe",""))
                throw new Exception();
        }


        public void Proxy()
        {
            var tpm = new TntSlavePropertyManager(new ITntProperty[] {
                new TntReadonlyProperty<int>("i", "i", 42),
                new TntValueProperty<double>("d", "d", true, 3.14),
                new TntDelegateProperty<int>("id", "id", OnRead, OnWrite),
            });
            var slaveContractHandler = new TntSlaveContractPropertyManager(tpm);
            
            var masterContractHandler = new TntMasterContractPropertyManager();
            masterContractHandler.ScanPropertiesCord = slaveContractHandler.GetDescriptionsCordHandler;

            masterContractHandler.Scan();
            
            if (!compareProperties(masterContractHandler["i"], tpm["i"]))
                throw new Exception();
            
            if (!compareProperties(masterContractHandler["d"], tpm["d"]))
                throw new Exception();

            if (!compareProperties(masterContractHandler["id"], tpm["id"]))
                throw new Exception();

            if (masterContractHandler.Properties.Length != tpm.Properties.Length)
                throw new Exception();

            masterContractHandler.SetPropertyValueCord += slaveContractHandler.SetValueCordHandler;
            masterContractHandler.GetPropertyValueCord += slaveContractHandler.GetValueCordHandler;

            masterContractHandler["d"].WriteValue(328.28);
            masterContractHandler["d"].UpdateValue();
            if (!compareProperties(masterContractHandler["d"], tpm["d"]))
                throw new Exception();

            
            if (!tpm.TrySetValue("d", "666"))
                throw new Exception();
            if (compareProperties(masterContractHandler["d"], tpm["d"]))
                throw new Exception();
        }

        bool compareProperties(ITntProperty a, ITntProperty b)
        {
            if (a.Name != b.Name)
                return false;
            if (a.TypeId != b.TypeId)
                return false;
            if (a.ValueType != b.ValueType)
                return false;
            if (a.IsWriteable != b.IsWriteable)
                return false;

            var araw = a.RawValue;
            var braw = b.RawValue;

            if (!araw.SequenceEqual(braw))
                return false;

            var aobj = a.GetObjectValue();
            var bobj = b.GetObjectValue();

            return aobj.Equals(bobj);
        }

        static int OnRead(object sender) {
            return 33;
        }

        static void OnWrite(object sender, int val) {}

    }
}
