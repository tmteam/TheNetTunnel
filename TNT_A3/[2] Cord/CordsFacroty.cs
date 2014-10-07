using System;
using System.Reflection;

namespace TheTunnel
{
	public static class CordFacroty
	{
	/*	public static IInCord CreateInMonoCord(Type argType, Type returnType, InAttribute attr)
		{
			IInCord ans = null;
			var des = DeserializersFactory.Create(argType);
			if (returnType == typeof(void)) {
				var gt = typeof(InCord<>).MakeGenericType (argType);
				ans = Activator.CreateInstance (gt, attr.CordId, des) as IInCord;
			} else {
				var ser = SerializersFactory.Create (returnType);
				ans = new AnsweringCord (attr.CordId, des, ser);
			}
			return ans;
		}


		public static IInCord AnswerCordFactory(Type argType, Type returnType, InAttribute attr)
		{
			IInCord ans = null;
			var des = DeserializersFactory.Create(argType);
			var ser = SerializersFactory.Create (returnType);
			ans = new AnsweringCord(attr.CordId, des, ser);
			return ans;
		}*/

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

		/*
		IOutCord OutCordFactory(PropertyInfo del, OutAttribute attr, object Contract)
		{
			var adel = del.PropertyType;
			var ainvk = adel.GetMethod ("Invoke");

			var parameters = ainvk.GetParameters ();



			if (parameters.Length == 1) {// monoparameter cord
				var cord = CreateOutMonoCord (parameters [0].ParameterType, ainvk.ReturnType, attr);
				var askcord = cord as IAskCord;

				if (askcord != null) {
					Delegate call = Delegate.CreateDelegate (del.PropertyType, askcord, "AskT");
					del.SetValue (Contract, call, null);
					AddInCord (askcord);
					AddOutCord (askcord);
				} else {
					Delegate call = Delegate.CreateDelegate (del.PropertyType, cord, "Send");
					del.SetValue (Contract, call, null);
					AddOutCord (cord);
				}
			} else { //Sequence serialization
				var returnType = ainvk.ReturnParameter.ParameterType;
				var argTypes = parameters.Select (p => p.ParameterType).ToArray ();
				if (returnType == typeof(void)) {
					var ocord = new OutCord<object[]> (attr.CordId, new SequenceSerializer (argTypes));

					var delegateHandler = HeavyReflectionTools.CreateConverterToArgsArrayAction(ocord.Send, argTypes);
					var convertedHandler = Delegate.CreateDelegate (adel, delegateHandler, "Invoke");
					del.SetValue (Contract, convertedHandler, null);

					AddOutCord (ocord);
				} else { // asking cord
					var gt  = typeof(AskCord<,>).MakeGenericType (returnType, typeof(object[]));
					var des = DeserializersFactory.Create (returnType);
					var acord = Activator.CreateInstance (gt, attr.CordId,
						new SequenceSerializer (argTypes), des) as IAskCord;

					var delegateHandler = HeavyReflectionTools.CreateConverterToArgsArrayFunc(acord.Ask, returnType, argTypes);
					var convertedHandler = Delegate.CreateDelegate (adel, delegateHandler, "Invoke");
					del.SetValue (Contract, convertedHandler, null);

					AddOutCord (acord);
					AddInCord (acord);
				}
			}
		}
		*/
		/*public static IAskCord AskCordFactory(Type argType, OutAttribute attr)
		{
			var des = DeserializersFactory.Create (returnType);
			var gt  = typeof(AskCord<,>).MakeGenericType (returnType, argType);
			ans = Activator.CreateInstance (gt, attr.CordId, ser, des) as IAskCord;
		}*/

	}
}

