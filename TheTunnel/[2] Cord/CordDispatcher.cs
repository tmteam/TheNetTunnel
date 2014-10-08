using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;


namespace TheTunnel
{
	public class CordDispatcher
	{
		public object Contract { get; protected set;}

		public CordDispatcher(object contract)
		{
			Senders = new Dictionary<short, IOutCord> ();
			Receivers = new Dictionary<short, IInCord> ();
			RegistrateContract(contract);
		}

		Dictionary<Int16,IOutCord> Senders;
		Dictionary<Int16, IInCord> Receivers;

		public void Handle(byte[] msg)
		{
			short INCid = BitConverter.ToInt16 (msg, 0);
			if (Receivers.ContainsKey (INCid))
				Receivers [INCid].Parse (msg, 2);
		}

		public void OnDisconnect(DisconnectReason reason)
		{
			var dscn = Contract as IDisconnectListener;
			if (dscn != null)
				dscn.OnDisconnect (reason);
		}

		public void AddInCord(IInCord cord)
		{
			if (Receivers.ContainsKey (cord.INCid))
				throw new ArgumentException ();
			Receivers.Add (cord.INCid, cord);
		}

		public void AddOutCord(IOutCord cord)
		{
			if (Senders.ContainsKey (cord.OUTCid))
				throw new ArgumentException ();
			Senders.Add (cord.OUTCid, cord);
			cord.NeedSend+= CordNeedSend;
		}

		public event Action<CordDispatcher, byte[]> NeedSend;

		void CordNeedSend (IOutCord arg1, byte[] msg)
		{
			send (msg);
		}

		void send(byte[] msg)
		{
			if (NeedSend != null) {
				NeedSend (this, msg);
			}
		}

		void RegistrateContract(object contract)
		{
			this.Contract = contract;
			var type = Contract.GetType ();

			var outCords = type
				.GetProperties ()
				.Select (p => new 
					{
						property = p, 
						attr = p.GetCustomAttributes (typeof(OutAttribute), true).FirstOrDefault () as OutAttribute
					})
				.Where (p => p.attr != null)
				.ToArray ();

			foreach (var oc in outCords)
				RegistrateOut (oc.property, oc.attr);


			var inCords = type
				.GetMethods ()
				.Select (m => new 
					{
						method = m, 
						attr = m.GetCustomAttributes (typeof(InAttribute), true).FirstOrDefault () as InAttribute 
					})
				.Where (m => m.attr != null)
				.ToArray ();

			foreach (var r in inCords) 
				RegistrateIn(r.method, r.attr);
		}

		void RegistrateOut(PropertyInfo del, OutAttribute attr)
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

		void RegistrateIn(MethodInfo meth, InAttribute attr)
		{
			var parameters = meth.GetParameters ();
			var returnType = meth.ReturnType;

			if (parameters.Length == 1) { //Usual monoparameter cord
				IInCord cord = CreateInMonoCord (parameters [0].ParameterType, returnType, attr);

				var answeringCord = cord as IAnsweringCord;
				if (answeringCord != null) { //If cord is answering
					answeringCord.OnAsk += (sender, id, msg) => {
						var res = meth.Invoke (Contract, new object[]{ msg });
						answeringCord.Answer (res, id);
					};
					AddInCord (answeringCord);
					AddOutCord (answeringCord);
				} else { // case of no-answer cord
					cord.OnReceive += (sender, msg) => meth.Invoke (Contract, new object[]{ msg });
					AddInCord (cord);
				}
			} else { //Sequence deserialization
				var types = parameters.Select (p => p.ParameterType).ToArray ();
				if (returnType == typeof(void)) {// no-answer cord
					var icord = new InCord<object[]> (attr.CordId, new SequenceDeserializer (types));
					icord.OnReceiveT += (sender, msg) => meth.Invoke (Contract, msg);
					AddInCord (icord);
				} else {// answering cord
					var ser = SerializersFactory.Create (returnType);
					var acord = new AnsweringCord (attr.CordId, new SequenceDeserializer (types), ser);
					acord.OnAsk += (sender, id, msg) => {
						var res = meth.Invoke (Contract, msg as object[]);
						acord.Answer (res, id);
					};
					AddInCord (acord);
					AddOutCord (acord);
				}
			}
		}

		static IInCord CreateInMonoCord(Type argType, Type returnType, InAttribute attr)
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

		static IOutCord CreateOutMonoCord(Type argType, Type returnType, OutAttribute attr)
		{
			IOutCord ans = null;
			var ser = SerializersFactory.Create (argType);
			if (returnType == typeof(void)) {
				var gt = typeof(OutCord<>).MakeGenericType (argType);
				ans = Activator.CreateInstance (gt, attr.CordId, ser) as IOutCord;
			} else {
				var des = DeserializersFactory.Create (returnType);
				var gt  = typeof(AskCord<,>).MakeGenericType (returnType, argType);
				ans = Activator.CreateInstance (gt, attr.CordId, ser, des) as IAskCord;
			}
			return ans;
		}
	}


	public enum DisconnectReason:byte
	{
		ContractWish = 0,
		UserWish = 1,
		ConnectionIsLost = 2,
	}
}

