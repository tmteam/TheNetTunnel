using System;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace Testing
{
	class MainClass
	{
        public static void Main(string[] args)
        {
            var test = new Test_FinalLightTunnel();
            
            try {
                SendReceiveTest();
                De_SerializationTest();
                CordDispatcherTest();
                FinalLightTunnelTest();
            } catch (Exception ex) {
                Console.WriteLine("Tests failed: " + ex.ToString());
            }

            Console.WriteLine("PAK2C..");
            Console.ReadKey();
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

			var test = new Test_De_Serialization ();
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

			var test = new Test_CordDispatcher ();
			test.PrimitiveJustOut ();
			test.ComplexJustOut ();
			test.PrimitiveAsk ();
			test.ComplexAsk ();
			test.EventPingPong ();

			Console.WriteLine("Succesfully");
		}

		public static void FinalLightTunnelTest()
		{
			Console.Write ("Testing Light Tunnel ...");

			var test = new Test_FinalLightTunnel ();
			
            test.ManyConnections ();
			test.PingPong ();
			test.RecursionCall ();
			test.CuteDDDOS ();
            test.ClientDisconnectHandling();
            test.ServerDisconnectHandling();
           
			Console.WriteLine("Succesfully");
		}
	}
}
