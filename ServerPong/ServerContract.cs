using System;
using TheTunnel;

namespace ServerPong{
	public class ServerContract{

        [In(1)]   public event Action<DateTime, int> ReceivePing;
       
        [Out(2)]  public  Action<DateTime, int> SendPong{get;set;}

        [In(3)]   public event Func<int, int> Question;
	}
}

