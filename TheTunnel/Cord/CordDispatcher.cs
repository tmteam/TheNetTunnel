using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;


namespace TheTunnel
{
	public class CordDispatcher2
	{
		public object Contract { get; protected set;}

		public CordDispatcher2(object contract)
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

		public void OnDisconnect()
		{

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

		void CordNeedSend (IOutCord arg1, byte[] msg)
		{
			send (msg);
		}

		public event Action<CordDispatcher2, byte[]> NeedSend;

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

			var aprm = ainvk.GetParameters ().FirstOrDefault();

			var cord = CreateOutCord (aprm.ParameterType, ainvk.ReturnType, attr);
			
			var askcord = cord as IAskCord;
		
			if(askcord!=null)
			{
				Delegate call = Delegate.CreateDelegate (del.PropertyType, askcord,  "AskT" );
				del.SetValue (Contract, call, null);
				AddInCord (askcord);
				AddOutCord(askcord);
			}
			else
			{
				Delegate call = Delegate.CreateDelegate (del.PropertyType, cord,  "Send" );
				del.SetValue (Contract, call, null);
				AddOutCord(cord);
			}

		}

		void RegistrateIn(MethodInfo meth, InAttribute attr)
		{
			var inputParameter = meth.GetParameters ().Select (p => p.ParameterType).FirstOrDefault ();

			var returnType = meth.ReturnType;

			var cord = CreateInCord (inputParameter, returnType, attr);

			var answeringCord = cord as IAnsweringCord;
			if (answeringCord != null) {
				answeringCord.OnAsk += (sender, id, msg) => {
					var res = meth.Invoke (Contract, new object[]{ msg });
					answeringCord.Answer (res, id);
				};
				AddInCord (answeringCord);
				AddOutCord (answeringCord);
			} else {
				cord.OnReceive += (sender, msg) => meth.Invoke (Contract, new object[]{ msg });
				AddInCord (cord);
			}
		}

		static IInCord CreateInCord(Type argType, Type returnType, InAttribute attr)
		{
			IInCord ans = null;
			if (returnType == typeof(void)) {
				var dt = typeof(ProtoDeserializer<>).MakeGenericType (argType);
				var des = Activator.CreateInstance (dt);
				var gt = typeof(InCord<>).MakeGenericType (argType);
				ans = Activator.CreateInstance (gt, attr.CordId, des) as IInCord;
			} else {
				var dt = typeof(ProtoDeserializer<>).MakeGenericType (argType);
				var des = Activator.CreateInstance (dt) as IDeserializer;
				var ser = new ProtoSerializer ();
				ans = new AnsweringCord (attr.CordId, des, ser);
			}
			return ans;
		}

		static IOutCord CreateOutCord(Type argType, Type returnType, OutAttribute attr)
		{
			IOutCord ans = null;
			if (returnType == typeof(void)) {
				var ser = new ProtoSerializer ();
				var gt = typeof(OutCord<>).MakeGenericType (argType);
				ans = Activator.CreateInstance (gt, attr.CordId, ser) as IOutCord;
			} else {
				var ser = new ProtoSerializer ();
				var dt = typeof(ProtoDeserializer<>).MakeGenericType(returnType);
				var des = Activator.CreateInstance (dt);
				var gt  = typeof(AskCord<,>).MakeGenericType (returnType, argType);

				ans = Activator.CreateInstance (gt, attr.CordId, ser, des) as IAskCord;
			}
			return ans;
		}
	}
}

