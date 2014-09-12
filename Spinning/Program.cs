using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using ProtoBuf;
namespace TheTunnel
{
	class Program
	{
		static void Main(string[] args)
		{


		}
	}
	[ProtoContract]
	public class someclass
	{
		[ProtoMember] public int i{ get; set; }
		[ProtoMember] public string[] strArr;
		[ProtoMember] public List<double> dblArr;
		[ProtoMember] public Dictionary<int, List<string>> dicIS;
	}
}
