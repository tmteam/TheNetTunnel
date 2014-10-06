using System;

namespace A3Expit
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			SendReceiveTest ();
		}

		public static void SendReceiveTest()
		{
			var test = new Light ();

			Console.WriteLine ("Short_SR");
			test.Short_SR ();
			Console.WriteLine ("Done");
			
			Console.WriteLine ();

			Console.WriteLine ("Long_SR");
			test.Long_SR ();
			Console.WriteLine ("Done");

			Console.WriteLine ();

			Console.WriteLine ("Huge_SR");
			test.Huge_SR ();
			Console.WriteLine ("Done");


			Console.WriteLine ("Short_FULLSR");
			test.Short_SRFULL ();
			Console.WriteLine ("Done");

			Console.WriteLine ();

			Console.WriteLine ("HUGE_FULLSR");
			test.Huge_FullSR ();
			Console.WriteLine ("Done");

			Console.WriteLine ();

			Console.WriteLine ("MultiSR");
			test.MultiSR ();
			Console.WriteLine ("Done");
		}
	}
}
