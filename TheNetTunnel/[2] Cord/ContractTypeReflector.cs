using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TNT.Cords
{
    /// <summary>
    /// Reflection info about contract's type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContractTypeReflector<T> where T: class, new()
    {
        public readonly InMethodDefenition[] InMethods;
        public readonly InEventDefenition[] InEvents;
        public readonly OutDelegateDefenition[] OutDeleagates;

        public ContractTypeReflector()
        {
            try
            {
                var type = typeof(T);
                var meths = new List<InMethodDefenition>();
                var events = new List<InEventDefenition>();
                var delegates = new List<OutDelegateDefenition>();
                foreach (var m in type.GetMembers())
                {
                    var p = m as PropertyInfo;
                    if (p != null) {
                        var outAttr = p.GetCustomAttributes(typeof(OutAttribute), true).FirstOrDefault() as OutAttribute;
                        if (outAttr != null) {
                            delegates.Add(new OutDelegateDefenition(p, outAttr));
                            continue;
                        }
                    }

                    var inAttr = m.GetCustomAttributes(typeof(InAttribute), true).FirstOrDefault() as InAttribute;
                    if (inAttr != null) {
                        var meth = m as MethodInfo;
                        if (meth != null)
                            meths.Add(new InMethodDefenition(meth, inAttr));
                        else {
                            var ev = m as EventInfo;
                            if (ev != null) {
                                var raiseField = type.GetField(ev.Name, BindingFlags.Instance | BindingFlags.NonPublic);
                                if (raiseField != null)
                                    events.Add(new InEventDefenition(raiseField, inAttr));
                            }
                        }
                    }
                }
                InMethods = meths.ToArray();
                InEvents = events.ToArray();
                OutDeleagates = delegates.ToArray();
            }
            catch (Exception ex) { }

        }

        public static readonly ContractTypeReflector<T> Reflector = new ContractTypeReflector<T>();
        
        public static void ParseAndBindContract<T>(T contract, out IInCord[] inputCords, out IOutCord[] outputCords)
        {
            var  ansInputCords = new List<IInCord>();
            var ansOutputCords = new List<IOutCord>();

            foreach(var o in Reflector.OutDeleagates){
                 var oCord = CordFacroty.OutCordFactory(o, contract);
                 ansOutputCords.Add(oCord);
                 var iCord = oCord as IInCord;
                 if (iCord != null)
                    ansInputCords.Add(iCord);
            }

            foreach(var m in Reflector.InMethods){
                var iCord = CordFacroty.InCordByMethodFactory(m, contract);
                ansInputCords.Add(iCord);
                var oCord = iCord as IOutCord;
                if(oCord!=null)
                    ansOutputCords.Add(oCord);
            }

            foreach(var e in Reflector.InEvents){
                var iCord = CordFacroty.InCordByEventFactory(e, contract);
                ansInputCords.Add(iCord);
                var oCord = iCord as IOutCord;
                if(oCord!=null)
                    ansOutputCords.Add(oCord); 
            }

            inputCords = ansInputCords.ToArray();
            outputCords = ansOutputCords.ToArray();
            
        }
    }

    public class InMethodDefenition
    {
        public InMethodDefenition(MethodInfo meth, InAttribute attr)
        {
            MethodDefenition = meth;
            Attribute = attr;
        }
        public readonly MethodInfo MethodDefenition;
        public readonly InAttribute Attribute;
    }

    public class InEventDefenition
    {
        public InEventDefenition(FieldInfo field, InAttribute attr)
        {
            RaiseField = field;
            Attribute = attr;
        }
        public readonly FieldInfo RaiseField;
        public readonly InAttribute Attribute;
    }

    public class OutDelegateDefenition
    {
        public OutDelegateDefenition(PropertyInfo prop, OutAttribute attr){
            DelegateProperty = prop;
            Attribute = attr;
        }
        public readonly PropertyInfo DelegateProperty;
        public readonly OutAttribute Attribute;
    }
}
