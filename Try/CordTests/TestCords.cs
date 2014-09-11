using System;
using TheTunnel;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

namespace TestingCord
{

	public class SimpleMessageCord: AskingCordBase<string, DateTime>
	{
		public SimpleMessageCord(string cordName, string answerCordName):base(cordName, answerCordName){}

		protected override DateTime Answer (string question)
		{
			Console.WriteLine ("Msg: " + question);
			return DateTime.Now;
		}

		protected override AnsweringCordBase<DateTime> CreateAnswerCord (string answerCordName)
		{
			return new SimpleAnswerCord (answerCordName);
		}

		protected override byte[] Serialize (string val, int valOffset)
		{
			byte[] ans = new byte[valOffset + val.Length * 4];
			Encoding.UTF32.GetBytes(val ,0,val.Length,ans,valOffset);
			return ans;
		}

		protected override bool TryDeserialize (byte[] qMsg, int offset, out string question)
		{
			if ((qMsg.Length - offset) % 4 != 0) {
				question = null;
				return false;
			} else {
				question = Encoding.UTF32.GetString (qMsg, offset, qMsg.Length - offset);
				return true;
			}	
		}
	}

	public class SimpleAnswerCord: TheTunnel.AnsweringCordBase<DateTime>
	{
		public SimpleAnswerCord(string name):base(name){}

		protected override byte[] Serialize (DateTime val, int valOffset)
		{
			byte[] ans = new byte[valOffset + 8];
			var res = BitConverter.GetBytes(val.ToBinary());
			res.CopyTo (ans, valOffset);
			return ans;
		}


		protected override bool TryDeserialize (byte[] qMsg, int offset, out DateTime answer)
		{
			if (qMsg.Length - offset < 8) {
				answer = DateTime.Now;
				return false;
			} else {
				answer = DateTime.FromBinary (BitConverter.ToInt64 (qMsg, offset));
				return false;
			}
		}
	}

	[StructLayout(LayoutKind.Explicit, Size= 64)]
	public class testMessage
	{

		[FieldOffset(0)]
		public int a;
		[FieldOffset(4)]
		public double b;
		[FieldOffset(12)]
		public float c;
		[FieldOffset(16)]
		public long d;


		public bool AreEqual(testMessage msg2)
		{
			return a == msg2.a && b == msg2.b && c == msg2.c && d == msg2.d;
		}
	}

	public class SayTestMessage: TheTunnel.SayFixedSizeObject<testMessage>
	{
		public SayTestMessage () : base ("tmsg"){}
	}
}

