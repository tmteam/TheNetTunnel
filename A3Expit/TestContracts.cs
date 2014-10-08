using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TheTunnel;

namespace A3Expit
{
	public class ActionContractA{

		[Out(1)] public Action<int> SendInt{get;set;}
		[Out(2)] public Action<int[]> SendIntArray{get;set;}

		[Out(3)] public Action<string> SendString{get;set;}
		[Out(4)] public Action<string[]> SendStringArray{get;set;}

		[Out(5)] public Action<ToiletType[]> SendProtobuffArray{get;set;}
		[Out(6)] public Action<int,double,short> SendFixedSequence{get;set;}
		[Out(7)] public Action<testEn,string[],ToiletType[]> SendComplexSequence{get;set;}
		[Out(8)] public Action SendVoid{ get; set;}
	}

	public class ActionContractB{
		public bool voidCalled;
		public int i;
		public testEn e;
		public int[] iArr;

		public string str;
		public string[] strArr;

		public double d;
		public short s;
		public ToiletType[] tt;

		[In(1)] public void ReceiveInt(int i){
			this.i = i;
		}
		[In(2)] public void ReceiveIntArray(int[] iArr){
			this.iArr = iArr;
		}

		[In(3)] public void ReceiveString(string s){
			this.str = s;
		}
		[In(4)] public void ReceiveStringArray(string[] sArr){
			this.strArr = sArr;
		}

		[In(5)] public void ReceiveProtoBuffArray(ToiletType[] tt){
			this.tt = tt;
		}

		[In(6)]public void ReceiveFixedSequence(int i, double d, short s)
		{
			this.i = i;
			this.d = d;
			this.s = s;
		}

		[In(7)]public void ReceiveComplexSequence(testEn e, string[] sArr, ToiletType[] tt)
		{
			this.e = e;
			this.strArr = sArr;
			this.tt = tt;
		}

		[In(8)] public void ReceiveVoid()
		{
			voidCalled = true;
		}
	}

	public class FuncContractA{

		[Out(1)] public Func<int, int> Twice{get;set;}
		[Out(2)] public Func<double[], double[], double> SummOf2Arrays{get;set;}
		[Out(3)] public Func<string,string,string,string[]> ToArray{get;set;}
		[Out(4)] public Func<string[],string[],string> Concat2Arrays{get;set;}
		[Out(5)] public Func<ToiletType,ToiletType> Copy{get;set;}
	}
	public class FuncContractB{
		[In(1)] public int Twice(int arg){
			return arg*2;
		}
		[In(2)] public double Summ(double[] arr1,double[] arr2){
			return arr1.Sum()+ arr2.Sum();
		}
		[In(3)] public string[] ToArray(string s1, string s2,string s3){
			return new string[]{ s1, s2, s3 };
		}
		[In(4)] public string Summ(string[] strArr1, string[] strArr2){
			return string.Concat (strArr1.Concat (strArr2).ToArray ());
		}
		[In(5)] public ToiletType Copy(ToiletType t){
			return t;
		}
	}
}

