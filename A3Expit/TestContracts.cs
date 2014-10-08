using System;
using TheTunnel;

namespace A3Expit
{
	public class TestContractA{

		[Out(1)] public Action<int> SendInt{get;set;}
		[Out(2)] public Action<string> SendString{get;set;}
		[Out(3)] public Action<string[]> SendStringArray{get;set;}

	}

	public class TestContractB{
		[In(1)] public void ReceiveInt(int i){
			Console.WriteLine ("iGet: " + i);
		}

		[In(2)] public void ReceiveString(string s){
			Console.WriteLine ("sGet: " + s);
		}

		[In(3)] public void ReceiveStringArr(string[] ss){
			Console.WriteLine ("ArrReceived");
			foreach(var s in ss)
				Console.WriteLine ("\t[]sGetarr: " + s);
		}

	}
}

