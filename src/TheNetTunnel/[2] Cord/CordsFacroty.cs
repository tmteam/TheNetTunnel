using System;
using System.Reflection;
using System.Linq;
using TheTunnel.Deserialization;
using TheTunnel.Serialization;

namespace TheTunnel.Cords
{
	public static class CordFacroty
	{
		public static IAnsweringCord AnswerCordFactory(Type[] argTypes, Type returnType, InAttribute attr)
		{
			var des = DeserializersFactory.Create(argTypes);
			var ser = SerializersFactory.Create (returnType);
			return  new AnsweringCord(attr.CordId, des, ser);
		}

		public static IAskCord AskCordFactory(Type[] argTypes, Type returnType,  OutAttribute attr)
		{
			var ser = SerializersFactory.Create(argTypes);
			var des = DeserializersFactory.Create (returnType);
			var gType = argTypes.Length == 1 ? argTypes [0] : typeof(object[]); 
			var gt  = typeof(AskCord<,>).MakeGenericType (returnType, gType);
			return Activator.CreateInstance (gt, attr.CordId, ser, des) as IAskCord;
		}

		public static IInCord JustInCordFactory(Type[] argTypes, InAttribute attr)
		{
			IInCord ans = null;
			var des = DeserializersFactory.Create(argTypes);
			var gType = argTypes.Length == 1 ? argTypes [0] : typeof(object[]); 
			var gt = typeof(InCord<>).MakeGenericType (gType);
			ans = Activator.CreateInstance (gt, attr.CordId, des) as IInCord;
			return ans;
		}

		public static IOutCord JustOutCordFactory(Type[] argTypes, OutAttribute attr)
		{
			IOutCord ans = null;
			var ser = SerializersFactory.Create (argTypes);
			var gType = argTypes.Length == 1 ? argTypes [0] : typeof(object[]); 
			var gt = typeof(OutCord<>).MakeGenericType (gType);
		    ans= Activator.CreateInstance (gt, attr.CordId, ser) as IOutCord;
			return ans;
		}

		public static IOutCord OutCordFactory(OutDelegateDefenition delegateInfo, object Contract)
		{
            var adel = delegateInfo.DelegateProperty.PropertyType;
			var ainvk = adel.GetMethod ("Invoke");
			var parameters = ainvk.GetParameters ().Select(p=>p.ParameterType).ToArray();
			Delegate call= null;
			IOutCord ans = null;

			if(ainvk.ReturnType== typeof(void))	{
				// Call
                var oCord = JustOutCordFactory(parameters, delegateInfo.Attribute);
				if (parameters.Length == 1)
                    call = Delegate.CreateDelegate(delegateInfo.DelegateProperty.PropertyType, oCord, "Send");
				else
				{
					var delegateHandler 
						= HeavyReflectionTools
							.CreateConverterToArgsArrayAction((oCord as IOutCord<object[]>).Send, parameters);
					call = Delegate.CreateDelegate (adel, delegateHandler, "Invoke");
				}
				ans = oCord;
			}
			else{
				// Question
                var aCord = AskCordFactory(parameters, ainvk.ReturnType, delegateInfo.Attribute);
				if(parameters.Length==1)
                    call = Delegate.CreateDelegate(delegateInfo.DelegateProperty.PropertyType, aCord, "AskT");
				else
				{
					var delegateHandler 
						= HeavyReflectionTools
							.CreateConverterToArgsArrayFunc((aCord as IAskCord).Ask, ainvk.ReturnType, parameters);
					call = Delegate.CreateDelegate (adel, delegateHandler, "Invoke");
				}
                aCord.MaxAwaitMs = (int)delegateInfo.Attribute.MaxAnswerAwaitInterval;
				ans = aCord;
			}
			delegateInfo.DelegateProperty.SetValue (Contract, call, null);
			return ans;
		}

		public static IInCord InCordByMethodFactory(InMethodDefenition methodInfo, object Contract)
		{
            var parameters = methodInfo.MethodDefenition.GetParameters().Select(p => p.ParameterType).ToArray();
            var returnType = methodInfo.MethodDefenition.ReturnType;

			if (parameters.Length == 1) { //Usual monoparameter cord
				if (returnType == typeof(void)) {
                    var ccord = JustInCordFactory(parameters, methodInfo.Attribute);
                    ccord.OnReceive += (sender, msg) => methodInfo.MethodDefenition.Invoke(Contract, new object[] { msg });
					return ccord;
				} else {
                    var acord = AnswerCordFactory(parameters, returnType, methodInfo.Attribute);
					acord.OnAsk += (sender, id, msg) => {
                        var res = methodInfo.MethodDefenition.Invoke(Contract, new object[] { msg });
						acord.SendAnswer (res, id);
					};
					return acord;
				}
			} else { //Sequence deserialization
				if (returnType == typeof(void)) {// no-answer cord
                    var icord = new InCord<object[]>(methodInfo.Attribute.CordId, new SequenceDeserializer(parameters));
                    icord.OnReceiveT += (sender, msg) => methodInfo.MethodDefenition.Invoke(Contract, msg);
					return icord;
				} else {// answering cord
					var ser = SerializersFactory.Create (returnType);
                    var acord = new AnsweringCord(methodInfo.Attribute.CordId, new SequenceDeserializer(parameters), ser);
					acord.OnAsk += (sender, id, msg) => {
                        var res = methodInfo.MethodDefenition.Invoke(Contract, msg as object[]);
						acord.SendAnswer (res, id);
                        };
                    
					return acord;
				}
			}
		}

		public static IInCord InCordByEventFactory(InEventDefenition raiseEventInfo, object Contract)
		{
            var meth = raiseEventInfo.RaiseField.FieldType.GetMethod("Invoke");

			var parameters = meth.GetParameters ().Select(p=>p.ParameterType).ToArray();
			var returnType = meth.ReturnType;

			if (parameters.Length == 1) { //Usual monoparameter cord
				if (returnType == typeof(void)) {
                    var ccord = JustInCordFactory(parameters, raiseEventInfo.Attribute);
					ccord.OnReceive += (sender, msg) => 
					{
                        var eventDelegate = raiseEventInfo.RaiseField.GetValue(Contract) as MulticastDelegate;

						if (eventDelegate != null)
							foreach (var handler in eventDelegate.GetInvocationList()) 
								handler.Method.Invoke (handler.Target, new object[] { msg });
					};
					return ccord;
				} else {
					var acord = AnswerCordFactory (parameters, returnType, raiseEventInfo.Attribute);
					acord.OnAsk += (sender, id, msg) => {
                        var eventDelegate = raiseEventInfo.RaiseField.GetValue(Contract) as MulticastDelegate;
						object ans = null;
						if (eventDelegate != null)
						{
							foreach (var handler in eventDelegate.GetInvocationList()) 
								ans = handler.Method.Invoke (handler.Target, new object[] { msg });
							acord.SendAnswer (ans, id);
						}
					};
					return acord;
				}
			} else { //Sequence deserialization
				if (returnType == typeof(void)) {// no-answer cord
                    var icord = new InCord<object[]>(raiseEventInfo.Attribute.CordId, new SequenceDeserializer(parameters));
					icord.OnReceiveT += (sender, msg) => {
                        var eventDelegate = raiseEventInfo.RaiseField.GetValue(Contract) as MulticastDelegate;
						if (eventDelegate != null)
							foreach (var handler in eventDelegate.GetInvocationList()) 
								handler.Method.Invoke (handler.Target, msg);
					};
					return icord;
				} else {// answering cord
					var ser = SerializersFactory.Create (returnType);
                    var acord = new AnsweringCord(raiseEventInfo.Attribute.CordId, new SequenceDeserializer(parameters), ser);
					acord.OnAsk += (sender, id, msg) => {
						object ans = null;
                        var eventDelegate = raiseEventInfo.RaiseField.GetValue(Contract) as MulticastDelegate;
						if (eventDelegate != null)
						{
							foreach (var handler in eventDelegate.GetInvocationList()) 
								ans = handler.Method.Invoke (handler.Target, msg as object[]);
							acord.SendAnswer(ans, id);
						}
					};
					return acord;
				}
			}
		}
	}
}

