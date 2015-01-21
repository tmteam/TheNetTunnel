using System;
using TheTunnel;

namespace Client{
	public class ClientContract{

        [Out(1)] public Action<DateTime, int> SendPing{get;set;}
        [Out(3)] public Func<int, int> Ask { get; set; }
        [In(2)]  public event Action<DateTime, int> ReceivePong;
	}
}

