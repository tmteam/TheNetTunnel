using System;
using System.Linq;

using TheTunnel;
using TheTunnel.Cords;

namespace Testing
{
	public class Test_CordDispatcher
	{
		public Test_CordDispatcher()
		{
			tt = Enumerable.Range (0, 100).Select (r => ToiletType.GetRandomType ()).ToArray ();
		}
		readonly int i = 42;
		readonly short s = 24;
		readonly double d = Math.PI;
		readonly testEn e = testEn.second;
		readonly string str = "FortyTwo";
		readonly string[] strArr = new string[]{"first", "second", "third"};
		readonly int[] iArr = new int[]{1,2,3,4,5,6,7,8,9,10,11};
		readonly ToiletType[] tt;

		public void PrimitiveJustOut()
		{
			var A = new ActionContractA ();
			var B = new ActionContractB ();

            Tools.ConnectContractsDirectly(A, B);
		
			A.SendInt (i);
			A.SendString (str);
			A.SendIntArray (iArr);
			A.SendStringArray(strArr);
			A.SendProtobuffArray (tt);

			if (B.i != i) 
				throw new Exception ("Primitive message send failed");
			if(B.str != str)
				throw new Exception ("Primitive message send failed");
			if(!B.iArr.SequenceEqual (iArr))
				throw new Exception ("Primitive message send failed");
			if(!B.strArr.SequenceEqual (strArr))
				throw new Exception ("Primitive message send failed");
			for (int j = 0; j < tt.Length; j++)
				if(!tt[j].IsEqual(B.tt[j]))
					throw new Exception ("Primitive message send failed");
		}

		public void ComplexJustOut()
		{
			var A = new ActionContractA ();
			var B = new ActionContractB ();

            Tools.ConnectContractsDirectly(A, B);

			A.SendVoid ();
			A.SendFixedSequence (i, d, s);
			A.SendComplexSequence (e, strArr, tt);

			if(!B.voidCalled)
				throw new Exception ("Complex message send failed");
			if(i!= B.i || d!=B.d|| s!= B.s)
				throw new Exception ("Complex message send failed");
			if(!B.strArr.SequenceEqual (strArr))
				throw new Exception ("Complex message send failed");
			for (int j = 0; j < tt.Length; j++)
				if(!tt[j].IsEqual(B.tt[j]))
					throw new Exception ("Complex message send failed");
		}

		public void PrimitiveAsk(){
			var A = new FuncContractA ();
			var B = new FuncContractB ();
            Tools.ConnectContractsDirectly(A, B);

			var twiceI = A.Twice (i);
			if (twiceI != i * 2)
				throw new Exception ("Primitive ask failed");

			var toiletCopy = A.Copy (tt.First());
			if(!toiletCopy.IsEqual(tt.First()))
				throw new Exception ("Primitive ask failed");
		}
		 
		public void ComplexAsk(){
			var A = new FuncContractA ();
			var B = new FuncContractB ();
            Tools.ConnectContractsDirectly(A, B);

			var dArr1 = Enumerable.Range (0, 100).Select(ii=>Convert.ToDouble(ii)).ToArray ();
			var dArr2 = Enumerable.Range (100, 200).Select(ii=>Convert.ToDouble(ii)).ToArray ();

			var summ = dArr1.Sum () + dArr2.Sum ();
			var res = A.SummOf2Arrays (dArr1, dArr2);
			if(summ!=res)
				throw new Exception ("Complex ask failed");

			var conc = A.ToArray (strArr [0], strArr [1], strArr[2]);

			if(!strArr.SequenceEqual(conc))
				throw new Exception ("Primitive ask failed");
				
			var strArr1 = dArr1.Select(a=>a.ToString()).ToArray ();
			var strArr2 = dArr2.Select(a=>a.ToString()).ToArray ();

			var sconc = string.Concat (strArr1.Concat (strArr2).ToArray ());

			if(A.Concat2Arrays(strArr1,strArr2).CompareTo(sconc)!=0)
				throw new Exception ("Primitive ask failed");
		
		}

		public void EventPingPong()
		{
			var A = new PingPongContract ();
			var B = new PingPongContract ();
            Tools.ConnectContractsDirectly(A, B);
			A.ReceivePong += (x, y) => {
				x++;
				return new ProtoPoint{ X = x, Y = y };
			};
			B.ReceivePong += (x, y) => {
				y++;
				return new ProtoPoint{ X = x, Y = y };
			};
			int cx = 0, cy = 0;
			for (int i = 0; i < 500; i++) {
				var pong = A.SendPing (cx, cy);
				pong = B.SendPing (pong.X, pong.Y);
				cx = pong.X;
				cy = pong.Y;
			}
			if (cx != 500 || cx != cy)
				throw new Exception ("EventPingPong fail");

		}

		
	
	}
}

