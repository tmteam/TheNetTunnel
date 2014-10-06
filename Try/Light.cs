using System;
using NUnit.Framework;
using TheTunnel;
using System.IO;

namespace Try
{
	[TestFixture] public class Light
	{
		[Test] public void SeparateAndCollectSimple()
		{

			var sep = new TheTunnel.LightSeparator ();

			var asm = new QuantumReceiver ();
			byte[] received = null;
			asm.OnLightMessage+= (QuantumReceiver arg1, QuantumHead arg2, MemoryStream arg3) => 
			{
				received = arg3.GetBuffer();
			};

			byte[] arr = new byte[]{ 1, 2, 3 };
			MemoryStream str = new MemoryStream (arr);
			sep.Initialize (arr, 42);
			while (sep.DataLeft > 0) {
				var snd = sep.Next ();
				asm.Set (snd);
			}
			Console.Write ("!");
		}
	}
}

