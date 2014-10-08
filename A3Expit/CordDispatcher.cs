using System;
using TheTunnel;

namespace A3Expit
{
	public class CordDispatcherTest
	{
		public void PrimitiveSendReceive()
		{
			TestContractA A = new TestContractA ();
			TestContractB B = new TestContractB ();

			CordDispatcher ACD = new CordDispatcher (A);
			CordDispatcher BCD = new CordDispatcher (B);

			ACD.NeedSend+= (CordDispatcher arg1, System.IO.MemoryStream arg2) => BCD.Handle(arg2);
			BCD.NeedSend+= (CordDispatcher arg1, System.IO.MemoryStream arg2) => ACD.Handle(arg2);

			A.SendInt (42);
			A.SendString ("FortyTwo");
			A.SendStringArray(new string[]{"first", "second", "third"});

		}
	}
}

