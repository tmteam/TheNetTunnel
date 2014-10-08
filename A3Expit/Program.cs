using System;

namespace A3Expit
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			SendReceiveTest ();
			De_SerializationTest ();
			CordDispatcherTest ();
		}

		public static void SendReceiveTest()
		{
			Console.WriteLine ("Testing Light ...\r\n{");

			var test = new Light ();
			test.Short_SR ();
			test.Long_SR ();
			test.Huge_SR ();
			test.Short_SRFULL ();
			test.Huge_FullSR ();
			test.MultiSR ();

			Console.WriteLine("}\r\nSuccesfully");
		}

		public static void De_SerializationTest()
		{
			Console.Write ("Testing De_serialization ...");

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

		public static void CordDispatcherTest()
		{
			Console.Write ("Testing CordDispatcher ...");

			var test = new CordDispatcherTest ();
			test.PrimitiveJustOut ();
			test.ComplexJustOut ();
			test.PrimitiveAsk ();
			test.ComplexAsk ();

			Console.WriteLine("Succesfully");
		}
	}
}
