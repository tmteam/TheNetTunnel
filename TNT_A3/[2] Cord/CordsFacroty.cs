using System;
using System.Reflection;
using System.Linq;

namespace TheTunnel
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

		public static IOutCord OutCordFactory(PropertyInfo del, OutAttribute attr, object Contract)
		{
			var adel = del.PropertyType;
			var ainvk = adel.GetMethod ("Invoke");
			var parameters = ainvk.GetParameters ().Select(p=>p.ParameterType).ToArray();
			Delegate call= null;
			IOutCord ans = null;

			if(ainvk.ReturnType== typeof(void))	{
				// Call
				var oCord = JustOutCordFactory (parameters, attr);
				if (parameters.Length == 1) 
					call = Delegate.CreateDelegate (del.PropertyType, oCord, "Send");
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
				var aCord = AskCordFactory (parameters, ainvk.ReturnType, attr);
				if(parameters.Length==1)
					call = Delegate.CreateDelegate (del.PropertyType, aCord, "AskT");
				else
				{
					var delegateHandler 
						= HeavyReflectionTools
							.CreateConverterToArgsArrayFunc((aCord as IAskCord).Ask, ainvk.ReturnType, parameters);
					call = Delegate.CreateDelegate (adel, delegateHandler, "Invoke");
				}
				aCord.MaxAwaitMs = (int)attr.MaxAnswerAwaitInterval;
				ans = aCord;
			}
			del.SetValue (Contract, call, null);
			return ans;
		}

		public static IInCord InCordFactory(MethodInfo meth, InAttribute attr, object Contract)
		{
			var parameters = meth.GetParameters ().Select(p=>p.ParameterType).ToArray();
			var returnType = meth.ReturnType;

			if (parameters.Length == 1) { //Usual monoparameter cord
				if (returnType == typeof(void)) {
					var ccord = JustInCordFactory (parameters, attr);
					ccord.OnReceive += (sender, msg) => meth.Invoke (Contract, new object[]{ msg });
					return ccord;
				} else {
					var acord = AnswerCordFactory (parameters, returnType, attr);
					acord.OnAsk += (sender, id, msg) => {
						var res = meth.Invoke (Contract, new object[]{ msg });
						acord.SendAnswer (res, id);
					};
					return acord;
				}
			} else { //Sequence deserialization
				if (returnType == typeof(void)) {// no-answer cord
					var icord = new InCord<object[]> (attr.CordId, new SequenceDeserializer (parameters));
					icord.OnReceiveT += (sender, msg) => meth.Invoke (Contract, msg);
					return icord;
				} else {// answering cord
					var ser = SerializersFactory.Create (returnType);
					var acord = new AnsweringCord (attr.CordId, new SequenceDeserializer (parameters), ser);
					acord.OnAsk += (sender, id, msg) => {
						var res = meth.Invoke (Contract, msg as object[]);
						acord.SendAnswer (res, id);
					};
					return acord;
				}
			}
		}

		public static IInCord InCordFactory(FieldInfo fieldOfEvent, InAttribute attr, object Contract)
		{
			var meth = fieldOfEvent.FieldType.GetMethod ("Invoke");

			var parameters = meth.GetParameters ().Select(p=>p.ParameterType).ToArray();
			var returnType = meth.ReturnType;

			if (parameters.Length == 1) { //Usual monoparameter cord
				if (returnType == typeof(void)) {
					var ccord = JustInCordFactory (parameters, attr);
					ccord.OnReceive += (sender, msg) => 
					{
						var eventDelegate = fieldOfEvent.GetValue (Contract) as MulticastDelegate;

						if (eventDelegate != null)
							foreach (var handler in eventDelegate.GetInvocationList()) 
								handler.Method.Invoke (handler.Target, new object[] { msg });
					};
					return ccord;
				} else {
					var acord = AnswerCordFactory (parameters, returnType, attr);
					acord.OnAsk += (sender, id, msg) => {
						var eventDelegate = fieldOfEvent.GetValue (Contract) as MulticastDelegate;
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
					var icord = new InCord<object[]> (attr.CordId, new SequenceDeserializer (parameters));
					icord.OnReceiveT += (sender, msg) => {
						var eventDelegate = fieldOfEvent.GetValue (Contract) as MulticastDelegate;
						if (eventDelegate != null)
							foreach (var handler in eventDelegate.GetInvocationList()) 
								handler.Method.Invoke (handler.Target, msg);
					};
					return icord;
				} else {// answering cord
					var ser = SerializersFactory.Create (returnType);
					var acord = new AnsweringCord (attr.CordId, new SequenceDeserializer (parameters), ser);
					acord.OnAsk += (sender, id, msg) => {
						object ans = null;
						var eventDelegate = fieldOfEvent.GetValue (Contract) as MulticastDelegate;
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

