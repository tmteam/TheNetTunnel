using System;
using System.Collections.Generic;
using System.IO;


namespace TNT.Cords
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
        		
        /// <summary>
        /// Handling input stream of light
        /// </summary>
        /// <param name="streamOfLight"></param>
        public void Handle(MemoryStream streamOfLight)
		{
            var idBuff = new byte[2];

			streamOfLight.Read (idBuff, 0, 2);

			var INCid = BitConverter.ToInt16 (idBuff, 0);
			if (Receivers.ContainsKey (INCid))
				Receivers [INCid].Parse (streamOfLight);
		}

		public void OnDisconnect(DisconnectReason reason){
			var dscn = Contract as IDisconnectListener;
			if (dscn != null)
				dscn.OnDisconnect (reason);

            foreach (var c in Senders)
                c.Value.Stop();
        }

		public void AddInputCord(IInCord cord){
			if (Receivers.ContainsKey (cord.INCid))
				throw new ArgumentException ();
			Receivers.Add (cord.INCid, cord);
		}

		public void AddOutputCord(IOutCord cord){
			if (Senders.ContainsKey (cord.OUTCid))
				throw new ArgumentException ();
			Senders.Add (cord.OUTCid, cord);
			cord.NeedSend+= outCord_needSend;
		}

		public event Action<object, MemoryStream> NeedSend;

		void outCord_needSend (IOutCord sender, MemoryStream stream, int length){
			if (NeedSend != null)
				NeedSend (this, stream);
		}
    }
}

