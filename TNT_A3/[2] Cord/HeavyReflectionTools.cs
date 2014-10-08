using System;

namespace TheTunnel
{
	/// <summary>
	/// Harcore reflection
	/// </summary>
	public static class HeavyReflectionTools
	{
		/// <summary>
		/// Creates the delegate, that handles  call with N arguments and call Action<object[]> with this arguments
		/// </summary>
		/// <returns>The converter to arguments array action.</returns>
		/// <param name="action">Action.</param>
		/// <param name="argTypes">Argument types.</param>
		public static Delegate CreateConverterToArgsArrayAction(Action<object[]> action, Type[] argTypes){

			Type t = null;
			switch (argTypes.Length){
			    case 0:  t = typeof(ActionCallConverterSubFactory);  break;
    			case 1:	 t = typeof(ActionCallConverterSubFactory<>); break;
			    case 2:	 t = typeof(ActionCallConverterSubFactory<,>); break;
				case 3:	 t = typeof(ActionCallConverterSubFactory<,,>); break;
				case 4:	 t = typeof(ActionCallConverterSubFactory<,,,>); break;
			default:
				throw new ArgumentException ("Arguments number should be less or equal 4");
//				case 5:	 t = typeof(ActionCallConverterSubFactory<,,,,>); break;
//				case 6:	 t = typeof(ActionCallConverterSubFactory<,,,,,>); break;
//				case 7:	 t = typeof(ActionCallConverterSubFactory<,,,,,,>); break;
//				case 8:	 t = typeof(ActionCallConverterSubFactory<,,,,,,,>); break;
//				case 9:	 t = typeof(ActionCallConverterSubFactory<,,,,,,,,>); break;
//				case 10: t = typeof(ActionCallConverterSubFactory<,,,,,,,,,>); break;
//				case 11: t = typeof(ActionCallConverterSubFactory<,,,,,,,,,,>); break;
//				case 12: t = typeof(ActionCallConverterSubFactory<,,,,,,,,,,,>); break;
//				case 13: t = typeof(ActionCallConverterSubFactory<,,,,,,,,,,,,>); break;
//				case 14: t = typeof(ActionCallConverterSubFactory<,,,,,,,,,,,,,>); break;
//				case 15: t = typeof(ActionCallConverterSubFactory<,,,,,,,,,,,,,,>); break;
//				case 16: t = typeof(ActionCallConverterSubFactory<,,,,,,,,,,,,,,,>); break;
			}

			var gt = t.MakeGenericType (argTypes);
			var gen = Activator.CreateInstance (gt) as IActionCallConverterSubFactory;
			return gen.GetActionConverter (action);
		}

		/// <summary>
		/// Creates the func delegate, that handles  call with N arguments and call Action<object[]> with this arguments
		/// </summary>
		/// <returns>The converter to arguments array action.</returns>
		/// <param name="action">Action.</param>
		/// <param name="argTypes">Argument types.</param>
		public static Delegate CreateConverterToArgsArrayFunc(Func<object[], object> func,Type returnType, Type[] argTypes)
		{
			Type t = null;
			switch (argTypes.Length){
				case 0:  t = typeof(FuncCallConverterSubFactory<>); break;
				case 1:	 t = typeof(FuncCallConverterSubFactory<,>); break;
				case 2:	 t = typeof(FuncCallConverterSubFactory<,,>); break;
				case 3:	 t = typeof(FuncCallConverterSubFactory<,,,>); break;
				case 4:	 t = typeof(FuncCallConverterSubFactory<,,,,>); break;
			default:
				throw new ArgumentException ("Arguments number should be less or equal 4");
//				case 5:	 t = typeof(FuncCallConverterSubFactory<,,,,,>); break;
//				case 6:	 t = typeof(FuncCallConverterSubFactory<,,,,,,>); break;
//				case 7:	 t = typeof(FuncCallConverterSubFactory<,,,,,,,>); break;
//				case 8:	 t = typeof(FuncCallConverterSubFactory<,,,,,,,,>); break;
//				case 9:	 t = typeof(FuncCallConverterSubFactory<,,,,,,,,,>); break;
//				case 10: t = typeof(FuncCallConverterSubFactory<,,,,,,,,,,>); break;
//				case 11: t = typeof(FuncCallConverterSubFactory<,,,,,,,,,,,>); break;
//				case 12: t = typeof(FuncCallConverterSubFactory<,,,,,,,,,,,,>); break;
//				case 13: t = typeof(FuncCallConverterSubFactory<,,,,,,,,,,,,,>); break;
//				case 14: t = typeof(FuncCallConverterSubFactory<,,,,,,,,,,,,,,>); break;
//				case 15: t = typeof(FuncCallConverterSubFactory<,,,,,,,,,,,,,,,>); break;
//				case 16: t = typeof(FuncCallConverterSubFactory<,,,,,,,,,,,,,,,,>); break;
			}

			var FuncTypes = new Type[argTypes.Length + 1];
			argTypes.CopyTo (FuncTypes, 0);
			FuncTypes [argTypes.Length] = returnType;  

			var gt = t.MakeGenericType (FuncTypes);
			var gen = Activator.CreateInstance (gt) as IFuncCallConverterSubFactory;
			return gen.GetFuncConverter (func);
		}
	}
}
