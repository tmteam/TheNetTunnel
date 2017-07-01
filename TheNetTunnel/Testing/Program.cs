using System;

namespace Testing
{
	class MainClass
	{
        public static void Main(string[] args)
        {
            //try {
              /*  SendReceiveTest();
                De_SerializationTest();
                CordDispatcherTest();
                FinalLightTunnelTest();*/
                TNTToolsTest();
                Console.WriteLine("\r\n(: All tests were done succesfully :)\r\n\r\nPAK2C..");
           
           /* }
            catch (Exception ex) {
                Console.WriteLine("Tests failed: " + ex.ToString());
            }*/
            
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
		    test.ByteEnumeration(2000);
            test.ByteEnumeration(10000);
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

	    public static void TNTToolsTest()
	    {
	        Console.WriteLine("TNT tools testing: ");
            Console.Write("Streams testing... ");

	        var streamTest = new Test_EnumerStreams();

	        streamTest.ReadonlyStreamOfFixedSizeEnumeration();
	        streamTest.StreamOfEnumeration();

            Console.WriteLine("[Succ]");
            
            Console.Write("Properties testing... ");

	        var test = new Test_Properties();
            
	        test.Creation();
	        test.Access();
	        test.Proxy();

            Console.WriteLine("[Succ]");
	    
        }
	}
}
