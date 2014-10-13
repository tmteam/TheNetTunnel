using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;


namespace TheTunnel.Cords
{
	public class CordDispatcher<T> where T: class, new()
	{
        public T Contract { get; protected set;}

		public CordDispatcher(T contract){
			Senders = new Dictionary<short, IOutCord> ();
			Receivers = new Dictionary<short, IInCord> ();

            IOutCord[] oCords = null;
            IInCord[] iCords = null;

            ContractTypeReflector<T>.ParseAndBindContract(contract, out iCords, out oCords);
            
            foreach (var o in oCords)
                AddOutputCord(o);
            foreach (var i in iCords)
                AddInputCord(i);

            this.Contract = contract;

		}

		Dictionary<Int16,IOutCord> Senders;
		Dictionary<Int16, IInCord> Receivers;

		byte[] idBuff = new byte[2];
        
        public void Handle(MemoryStream streamOfLight)
		{
			streamOfLight.Read (idBuff, 0, 2);

			short INCid = BitConverter.ToInt16 (idBuff, 0);
			if (Receivers.ContainsKey (INCid))
				Receivers [INCid].Parse (streamOfLight);
		}

		public void OnDisconnect(DisconnectReason reason)
		{
			var dscn = Contract as IDisconnectListener;
			if (dscn != null)
				dscn.OnDisconnect (reason);
		}

		public void AddInputCord(IInCord cord)
		{
			if (Receivers.ContainsKey (cord.INCid))
				throw new ArgumentException ();
			Receivers.Add (cord.INCid, cord);
		}

		public void AddOutputCord(IOutCord cord)
		{
			if (Senders.ContainsKey (cord.OUTCid))
				throw new ArgumentException ();
			Senders.Add (cord.OUTCid, cord);
			cord.NeedSend+= outCord_needSend;
		}

		public event Action<object, MemoryStream> NeedSend;

		void outCord_needSend (IOutCord sender, MemoryStream stream, int length)
			{
			if (NeedSend != null) {
				NeedSend (this, stream);
			}
		}
           /*
        void parseContract(T contract)
        {

          
         
			this.Contract = contract;
			var type = Contract.GetType ();

			#region delegates->out

			var outCords = type
				.GetProperties ()
				.Select (p => new 
					{
						property = p, 
						attr = p.GetCustomAttributes (typeof(OutAttribute), true).FirstOrDefault () as OutAttribute
					})
				.Where (p => p.attr != null)
				.ToArray ();

			foreach (var oc in outCords) {
				var oCord = CordFacroty.OutCordFactory (oc.property, oc.attr, contract);
				AddOutputCord (oCord);
				var iCord =  oCord as IInCord;
				if (iCord != null)	AddInputCord (iCord);
			}
			#endregion


			#region input->methods

			var inCordsMethods = type
				.GetMethods ()
				.Select (m => new 
					{
						method = m, 
						attr = m.GetCustomAttributes (typeof(InAttribute), true).FirstOrDefault () as InAttribute 
					})
				.Where (m => m.attr != null)
				.ToArray ();

			foreach (var r in inCordsMethods) {
				var iCord = CordFacroty.InCordByMethodFactory (r.method, r.attr, contract);
				AddInputCord (iCord);
				var oCord = iCord as IOutCord;
				if (oCord != null)	AddOutputCord (oCord);
			}
			#endregion


			#region input->events

			var inCordsEvents = type
				.GetEvents()
				.Select (f => new 
					{
						field = type.GetField(f.Name, BindingFlags.Instance | BindingFlags.NonPublic), 
						attr = f.GetCustomAttributes (typeof(InAttribute), true).FirstOrDefault () as InAttribute 
					})
				.Where (m => m.attr != null)
				.ToArray ();

			foreach (var r in inCordsEvents) {
				var iCord = CordFacroty.InCordByEventFactory (r.field, r.attr, contract);
				AddInputCord (iCord);
				var oCord = iCord as IOutCord;
				if (oCord != null)	AddOutputCord (oCord);
			}
			#endregion
		
        }
        */
    }
}

