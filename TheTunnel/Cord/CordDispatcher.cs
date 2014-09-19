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
			RegistrateContract(contract);
		}

		Dictionary<Int16,IOutCord> Senders;


		public void Receive(byte[] msg)
		{
		}

		public void OnDisconnect()
		{
		}
		public void AddInCord(IInCord cord)
		{


		}
		public void AddOutCord(IOutCord cord)
		{
			if (Senders.ContainsKey (cord.Cid))
				throw new ArgumentException ();
			Senders.Add (cord.Cid, cord);
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
			}
			else
			{
				Delegate call = Delegate.CreateDelegate (del.PropertyType, cord,  "Send" );
				del.SetValue (Contract, call, null);
			}
			AddOutCord(cord);
		}

		void RegistrateIn(MethodInfo meth, InAttribute attr)
		{
			/*
			var type = Contract.GetType ();

			var ReceiveCords = type
				.GetMethods ()
				.Select (m => new 
					{
						method = m, 
						attr = m.GetCustomAttributes (typeof(InAttribute), true).FirstOrDefault () as InAttribute 
					})
				.Where (m => m.attr != null)
				.ToArray ();
		
			foreach (var r in ReceiveCords) {
*/
				var inputParameter = meth.GetParameters ().Select (p => p.ParameterType).FirstOrDefault ();

				var returnType = meth.ReturnType;

				var cord = CreateInCord (inputParameter, returnType, attr);

				var ansCord = cord as IAnswerCord;
				if (ansCord != null) {
					cord.OnReceive += (sender, msg) => {
						var res = meth.Invoke (Contract, new object[]{ msg });
						ansCord.Answer(res);
					};
				} else 
					cord.OnReceive += (sender, msg)=> meth.Invoke (Contract, new object[]{ msg });

				AddInCord (ansCord);
				//}
		}

		static IInCord CreateInCord(Type argType, Type returnType, InAttribute attr)
		{
			IInCord ans = null;
			if (returnType == typeof(void)) {
				var dt = typeof(ProtoDeserializer<>).MakeGenericType(argType);
				var des = Activator.CreateInstance (dt);
				var gt = typeof(InCord<>).MakeGenericType (argType);
				ans = Activator.CreateInstance (gt, attr.CordId, des);
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

