using System;
using NUnit.Framework;
using TheTunnel;
namespace Try
{
	[TestFixture] public class SendReceiveSequence
	{
		CordDispatcher Ad;
		CordDispatcher Bd;
		AContract A;
		BContract B;

		bool IsInitialized = false;
		public SendReceiveSequence ()
		{

		}
		[Test] public void Initialization()
		{
			if (IsInitialized)
				return;
			A = new AContract ();
			B = new BContract ();

			Ad = new CordDispatcher (A);
			Bd = new CordDispatcher (B);

			Ad.NeedSend+= (sender, msg) => Bd.Handle(msg);
			Bd.NeedSend+= (sender, msg) => Ad.Handle(msg);
			IsInitialized = true;
		}
		[Test] public void string_0(){
			Initialization ();
			var ret = B.r_Snd0 ();
			if (ret != "0")	throw new Exception ("wrong Receive");
		}
		[Test] public void double_2(){
			Initialization ();
			double d1 = 100, d2 = 25.123456; 
			var ret = B.r_Snd2 (d1,d2);
			if (ret != d1+d2)	throw new Exception ("wrong Receive");
		}
		[Test] public void string_3(){
			Initialization ();

			AContract localContract = new AContract();
			double d1 = 100.123456;
			int i2 = 25;
			string hi = "hiiiii";

			var ret = B.r_Snd3 (d1,i2,hi);

			if (ret != localContract.r_Rec3(d1,i2,hi))	throw new Exception ("wrong Receive");
		}

	}


	public class AContract	{
		[In(0)] public string r_Rec0(){
			return "0";
		}
		[In(2)] public double r_Rec2(double d1, double d2){
			return d1 + d2;
		}
		[In(3)] public string r_Rec3(double d1, int i2, string s3){
			return d1.ToString () + i2 + s3;
		}
		[In(4)] public string r_Rec4(double d1, int i2, string s3, DateTime dt4){
			return d1.ToString () + i2 + s3+ dt4;
		}
		[In(5)] public DateTime r_Rec5(long binarystart, int H, double M, byte S, uint Ms){
			return DateTime.FromBinary (binarystart) + new TimeSpan (0, (int)H, (int)M, (int)S, (int)Ms);
		}
		[In(6)] public DateTime r_Rec6(byte[] binarystart, string D, string H, string M, string S, string Ms){
			return DateTime.FromBinary (BitConverter.ToInt64(binarystart,0)) + new TimeSpan (int.Parse(D), int.Parse(H), int.Parse(M), int.Parse(S), int.Parse(Ms));
		}
	}
	public class BContract{
		[Out(0)] public Func<string> r_Snd0{get;set;}
		[Out(2)] public Func<double, double, double> r_Snd2{get;set;}
		[Out(3)] public Func<double, int, string, string> r_Snd3{get;set;}
		[Out(4)] public Func<double, int, string, DateTime, string> r_Snd4{get;set;}
		[Out(5)] public Func<long, int, double, byte, uint,DateTime> r_Snd5{get;set;}
		[Out(6)] public Func<byte[], string, string ,string ,string, string, DateTime> r_Snd6{get;set;}
	}
	delegate void del3(string s1, DateTime d2, double d3); 
	delegate string del7(string s1, DateTime d2, double d3, byte[] b4, int i5, string s6, double[]i7); 

}

