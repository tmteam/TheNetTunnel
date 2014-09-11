using System;
using System.Text;
using System.Threading;

namespace TheTunnel
{
	public abstract class CordBase: ICord
	{
		public CordBase(string cordName){
			Name = cordName;
			if (Name.Length != 4)
				throw new ArgumentException ("Cord name's lenght must be equal 4");
			BName = Encoding.ASCII.GetBytes (Name);
		} 

		protected void RaiseNeedSend(byte[] qMsg)
		{
			if(NeedSend!=null)
				NeedSend(this, qMsg);
		}

		public event Action<ICord, byte[]> NeedSend;

		public abstract bool Parse (byte[] qMsg);

		public byte[] BName {get; private set;}

		public string Name { get; private set;}
	}

}

