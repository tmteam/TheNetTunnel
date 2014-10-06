using System;
using System.IO;
using TheTunnel;
using System.Diagnostics;
using System.Collections.Generic;

namespace A3Expit
{
	public class Light
	{
		public Random rnd = new Random(DateTime.Now.Millisecond);

		MemoryStream GetRandomStream(int length)
		{
			MemoryStream ans = new MemoryStream ();
			int maxArrSize = 1000;
			while (ans.Length < length) {
				byte[] buff = new byte[maxArrSize];
				rnd.NextBytes (buff);
				ans.Write (buff, 0, (int)Math.Min (length-ans.Length, buff.Length));
			}
			return ans;
		}

		bool CompareStreams (MemoryStream a, MemoryStream b)
		{
			a.Position = 0;
			b.Position = 0;
			while (a.Position < a.Length) {
				var Bsended = a.ReadByte ();
				var Breceived = b.ReadByte ();
				if (Bsended != Breceived)
					return false;
			}
			return true;
		}

		public void Short_SR()
		{
			var testStream = new MemoryStream (new byte[]{ 1, 2, 3, 4, 5 });
			TestSendAndReceiveBasics (testStream);
		}

		public void Short_SRFULL()
		{
			var testStream = new MemoryStream (new byte[]{ 1, 2, 3, 4, 5 });
			TestSingleSendAndReceive (testStream);
		}

		public void Long_SR()
		{
			var testStream = new MemoryStream ();
			byte[] arr = new byte[byte.MaxValue];
			//about 200K;
			for(int j = 0; j<1000; j++)
			{
				for (byte i = 0; i < byte.MaxValue; i++) 
					arr [i] = i;
			
				testStream.Write (arr, 0, arr.Length);
			}
			testStream.Position = 0;
			TestSendAndReceiveBasics (testStream);
		}

		public void Huge_SR()
		{
			var testStream = new MemoryStream ();
			byte[] arr = new byte[byte.MaxValue];
			//about 25M;
			for(int j = 0; j<100000; j++)
			{
				for (byte i = 0; i < byte.MaxValue; i++) 
					arr [i] = i;

				testStream.Write (arr, 0, arr.Length);
			}
			testStream.Position = 0;
			TestSendAndReceiveBasics (testStream);
		}

		public void Huge_FullSR()
		{
			var testStream = new MemoryStream ();
			byte[] arr = new byte[byte.MaxValue];
			//about 25M;
			for(int j = 0; j<100000; j++)
			{
				for (byte i = 0; i < byte.MaxValue; i++) 
					arr [i] = i;

				testStream.Write (arr, 0, arr.Length);
			}
			testStream.Position = 0;
			TestSingleSendAndReceive (testStream);
		}

		public void MultiSR()
		{

			var s0 = GetRandomStream (1);
			var s1 = GetRandomStream (10);
			var s2 = GetRandomStream (100);
			var s3 = GetRandomStream (1000);
			var s4 = GetRandomStream (10000);
			var s5 = GetRandomStream (100000);
			var s6 = GetRandomStream (1000000);
			var s7 = GetRandomStream (10000000);
			var s8 = GetRandomStream (100000000);

			TestMultipleSendAndReceive(new MemoryStream[]{s0,s1,s2,s3,s4,s5,s6, s7, s8});
		}

		public void TestSendAndReceiveBasics(MemoryStream sendStream)
		{
			Console.WriteLine ("Test Send And Receive for size " + sendStream.Length+" b");
			var sep = new TheTunnel.LightSeparator ();

			var asm = new QuantumReceiver ();

			MemoryStream received = null;

			asm.OnLightMessage+= (QuantumReceiver arg1, QuantumHead arg2, MemoryStream arg3) => 
			{
				received = new MemoryStream();
				arg3.WriteTo(received);
			};
			asm.OnCollectingError += (QuantumReceiver arg1, QuantumHead arg2, byte[] arg3) => {
				throw new Exception("Error during collecting");
			};
			Stopwatch tmr = new Stopwatch ();
			tmr.Start ();

			sep.Initialize (sendStream, 42);
			while (sep.DataLeft > 0) {
				var snd = sep.Next (500);
				asm.Set (snd);
			}

			if (received==null)
				throw new Exception ("No Receive");

			if (received.Length != sendStream.Length)
				throw new Exception ("Sended and received size are not equal");

			received.Position = 0;
			sendStream.Position = 0;

			tmr.Stop ();
			while (sendStream.Position < sendStream.Length) {
				var Bsended = sendStream.ReadByte ();
				var Breceived = received.ReadByte ();
				if (Bsended != Breceived)
					throw new Exception ("received and sended values are not equal");
			}

			Console.WriteLine ("SR done in " + tmr.ElapsedMilliseconds);

		}

		public void TestSingleSendAndReceive(MemoryStream lightMessage)
		{
			var sender = new QuantumSender ();
			var receiver = new QuantumReceiver ();

			MemoryStream received = null;
			receiver.OnLightMessage += (QuantumReceiver arg1, QuantumHead arg2, MemoryStream arg3) => {
				received = arg3;
			};
			receiver.OnCollectingError += (QuantumReceiver arg1, QuantumHead arg2, byte[] arg3) => {
				throw new Exception("Error during collecting");
			};

			Stopwatch tmr = new Stopwatch ();
			tmr.Start ();

			sender.Set (lightMessage);

			int MaxQuantumSize = 500;
			byte[] Quantum;
			int msgId;

			while (sender.TryNext (MaxQuantumSize, out Quantum, out msgId)) {
				if (Quantum == null || Quantum.Length == 0)
					throw new Exception ("Wrong quant size");
				receiver.Set (Quantum);
			}
			if (received == null)
				throw new Exception ("no receiving");

			received.Position = 0;
			lightMessage.Position = 0;

			tmr.Stop ();
			while (lightMessage.Position < lightMessage.Length) {
				var Bsended = lightMessage.ReadByte ();
				var Breceived = received.ReadByte ();
				if (Bsended != Breceived)
					throw new Exception ("received and sended values are not equal");
			}
			Console.WriteLine ("Full Mono SR["+lightMessage.Length +"] done in " + tmr.ElapsedMilliseconds);
		}

		public void TestMultipleSendAndReceive(MemoryStream[] messages)
		{
			Dictionary<int, MemoryStream> streams = new Dictionary<int, MemoryStream> ();
			foreach (var m in messages) {
				if (streams.ContainsKey ((int)m.Length))
					throw new Exception ("Incorrect test argumenrs. Streams must have an unique size");
				streams.Add ((int)m.Length, m);
			}

			var sender = new QuantumSender ();
			var receiver = new QuantumReceiver ();

			List<MemoryStream> received = new List<MemoryStream> ();
			receiver.OnLightMessage += (QuantumReceiver arg1, QuantumHead arg2, MemoryStream arg3) => {
				received.Add(arg3);
			};

			receiver.OnCollectingError += (QuantumReceiver arg1, QuantumHead arg2, byte[] arg3) => {
				throw new Exception("Error during collecting");
			};


			int MaxQuantumSize = 500;
			byte[] Quantum;
			int msgId;

			MemoryStream TransportStream = new MemoryStream ();

			Stopwatch tmr = new Stopwatch ();
			tmr.Start ();

			foreach (var m in messages) {
				m.Position = 0;
				sender.Set (m);
				if(sender.TryNext (MaxQuantumSize, out Quantum, out msgId)) {
					if (Quantum == null || Quantum.Length == 0)
						throw new Exception ("Wrong quant size");
					TransportStream.Write(Quantum,0,Quantum.Length);
				}
			}

			while (sender.TryNext (MaxQuantumSize, out Quantum, out msgId)) {
				if (Quantum == null || Quantum.Length == 0)
					throw new Exception ("Wrong quant size");
				TransportStream.Write(Quantum,0,Quantum.Length);
			}
			//Immitate Stream-Style Delivery;
			TransportStream.Position = 0;
			while (TransportStream.Position < TransportStream.Length) {
				var size = rnd.Next () % (MaxQuantumSize * 5);
				size = (int)Math.Min (TransportStream.Length - TransportStream.Position, size);
				byte[] msg = new byte[size];
				TransportStream.Read (msg, 0, size);
				receiver.Set (msg);
			}
			tmr.Stop ();
			//Check Received:
			if (received == null)
				throw new Exception ("no receiving");

			if (received.Count != streams.Count)
				throw new Exception ("Not all messages Received");

			int totalBytesCount=0;
			foreach (var r in received) {
				totalBytesCount += (int)r.Length;

				if (!streams.ContainsKey ((int)r.Length))
					throw new Exception ("incorrect length received");
				var snd = streams [(int)r.Length];
				if (!CompareStreams (snd, r))
					throw new Exception ("Sended and received messages are not equal");
				streams.Remove ((int)r.Length);
			}
			Console.WriteLine ("Send receive operation for " + totalBytesCount + " was done in " + tmr.ElapsedMilliseconds);
		}
	}
}

