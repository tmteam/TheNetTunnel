using System;

namespace A3Expit
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//SendReceiveTest ();
			De_SerializationTest ();
			//CordDispatcherTest cdt = new CordDispatcherTest ();
			//cdt.PrimitiveSendReceive ();
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

		public static void De_SerializationTest()
		{
			Console.Write ("De_serialization test...");

			var test = new De_Serialization ();

			test.Primitive ();
			test.Enum ();
			test.Unicode ();
			test.UTCFileTime ();
			test.ProtoBuf ();
			test.FixedSizeArrays ();
			test.DynamicSizeArrays ();
			test.Sequence ();

			Console.WriteLine("Succesfully");
		}
	}
}
