using System;

namespace TheTunnel.Cords
{
	interface IActionCallConverterSubFactory
	{
		Delegate GetActionConverter (Action<object[]> act);
	}

	//almost strangest code i've ever wrote:
	class ActionCallConverterSubFactory: IActionCallConverterSubFactory{
		public Delegate GetActionConverter (Action<object[]> act){
			Action ans = ()=> act(new object[0]);
			return ans;
		}
	}
	class ActionCallConverterSubFactory<T1>: IActionCallConverterSubFactory{
		public Delegate GetActionConverter (Action<object[]> act){
			Action<T1> ans = (t1)=> act(new object[]{t1});
			return ans;
		}
	}
	class ActionCallConverterSubFactory<T1,T2>: IActionCallConverterSubFactory{
		public Delegate GetActionConverter (Action<object[]> act){
			Action<T1,T2> ans = (t1,t2)=> act(new object[]{t1,t2});
			return ans;
		}
	}
	class ActionCallConverterSubFactory<T1,T2,T3>: IActionCallConverterSubFactory{
		public Delegate GetActionConverter (Action<object[]> act){
			Action<T1,T2,T3> ans = (t1,t2,t3)=> act(new object[]{t1,t2,t3});
			return ans;
		}
	}
	class ActionCallConverterSubFactory<T1,T2,T3,T4>: IActionCallConverterSubFactory{
		public Delegate GetActionConverter (Action<object[]> act){
			Action<T1,T2,T3,T4> ans = (t1,t2,t3,t4)=> act(new object[]{t1,t2,t3,t4});
			return ans;
		}
	}

//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5>: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5> ans = (t1,t2,t3,t4,t5)=> act(new object[]{t1,t2,t3,t4,t5});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6>: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6> ans = (t1,t2,t3,t4,t5,t6)=> act(new object[]{t1,t2,t3,t4,t5,t6});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7>: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7> ans = (t1,t2,t3,t4,t5,t6,t7)=> act(new object[]{t1,t2,t3,t4,t5,t6,t7});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7,T8>
//		: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7,T8> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8)
//				=> act(new object[]{t1,t2,t3,t4,t5,t6,t7,t8});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7,T8,T9>
//		: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9)
//				=> act(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>
//		: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10)
//				=> act(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>
//		: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11)
//				=> act(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>
//		: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12)
//				=> act(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>
//		: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13)
//				=> act(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>
//		: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14)
//				=> act(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>
//		: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15)
//				=> act(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15});
//			return ans;
//		}
//	}
//	class ActionCallConverterSubFactory<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>
//		: IActionCallConverterSubFactory{
//		public Delegate GetActionConverter (Action<object[]> act){
//			Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12, T13,T14,T15,T16> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15,t16)
//				=> act(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15,t16});
//			return ans;
//		}
//	}
}

