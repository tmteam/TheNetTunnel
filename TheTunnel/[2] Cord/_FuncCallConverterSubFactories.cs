using System;

namespace TheTunnel
{
	interface IFuncCallConverterSubFactory
	{
		Delegate GetFuncConverter (Func<object,object> fnc);
	}
	//Most strange code i've ever write# 2:
	class FuncCallConverterSubFactory<OutT>: IFuncCallConverterSubFactory{
		public Delegate GetFuncConverter (Func<object, object> fnc){
			Func<OutT> ans = ()=> (OutT)fnc(new object[0]);
			return ans;
		}
	}
	class FuncCallConverterSubFactory< T1,OutT>: IFuncCallConverterSubFactory{
		public Delegate GetFuncConverter (Func<object, object> fnc){
			Func< T1, OutT> ans = (t1)=> (OutT)fnc(new object[]{t1});
			return ans;
		}
	}
	class FuncCallConverterSubFactory< T1,T2,OutT>: IFuncCallConverterSubFactory{
		public Delegate GetFuncConverter (Func<object, object> fnc){
			Func< T1,T2, OutT> ans = (t1,t2)=> (OutT)fnc(new object[]{t1,t2});
			return ans;
		}
	}
	class FuncCallConverterSubFactory< T1,T2,T3,OutT>: IFuncCallConverterSubFactory{
		public Delegate GetFuncConverter (Func<object, object> fnc){
			Func< T1,T2,T3, OutT> ans = (t1,t2,t3)=> (OutT)fnc(new object[]{t1,t2,t3});
			return ans;
		}
	}
	class FuncCallConverterSubFactory< T1,T2,T3,T4,OutT>: IFuncCallConverterSubFactory{
		public Delegate GetFuncConverter (Func<object, object> fnc){
			Func< T1,T2,T3,T4, OutT> ans = (t1,t2,t3,t4)=> (OutT)fnc(new object[]{t1,t2,t3,t4});
			return ans;
		}
	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,OutT>: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5, OutT> ans = (t1,t2,t3,t4,t5)=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,OutT>: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6, OutT> ans = (t1,t2,t3,t4,t5,t6)=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,OutT>: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7, OutT> ans = (t1,t2,t3,t4,t5,t6,t7)=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,T8,OutT>
//		: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7,T8,OutT> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8)
//				=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7,t8});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,T8,T9,OutT>
//		: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7,T8,T9,OutT> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9)
//				=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,OutT>
//		: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,OutT> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10)
//				=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,OutT>
//		: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,OutT> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11)
//				=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,OutT>
//		: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,OutT> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12)
//				=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,OutT>
//		: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,OutT> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13)
//				=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,OutT>
//		: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,OutT> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14)
//				=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,OutT>
//		: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,OutT> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15)
//				=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15});
//			return ans;
//		}
//	}
//	class FuncCallConverterSubFactory< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,OutT>
//		: IFuncCallConverterSubFactory{
//		public Delegate GetFuncConverter (Func<object, object> fnc){
//			Func< T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12, T13,T14,T15,T16,OutT> ans 
//			= (t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15,t16)
//				=> (OutT)fnc(new object[]{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15,t16});
//			return ans;
//		}
//	}
}

