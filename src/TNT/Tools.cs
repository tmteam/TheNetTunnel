using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TNT
{
    public static class Tools
    {
		public static void CopyToAnotherStream(this Stream stream,  Stream targetStream,int lenght)
		{
			int lasts = lenght;
			while (lasts>0) {

				byte[] arr = new byte[lasts > 4096 ? 4096 : lasts];
				stream.Read (arr, 0, arr.Length);
				targetStream.Write (arr, 0, arr.Length);
				lasts -= arr.Length;
			}
		}
		public static void WriteToStream<T>(this T str, Stream stream, int size = -1) 
		{
			if(size==-1)
				size = Marshal.SizeOf(str);
			var arr = new byte[size];
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(str, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);
			stream.Write (arr, 0, size);
		}

        public static void SetToArray<T>(this T str, byte[] array, int dest,  int size = -1) 
        {
            if(size==-1)
                size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, array, dest, size);
            Marshal.FreeHGlobal(ptr);
        }

        public static T ToStruct<T>(this byte[] array, int src, int size = -1)
        {
            if (size == -1)
                size = Marshal.SizeOf(typeof(T));
            IntPtr p = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, src, p, size);
            T ans = (T)Marshal.PtrToStructure(p, typeof(T));
            Marshal.FreeHGlobal(p);
            return ans;
        }
		public static Delegate CreateFuncTTDelegate(Type inType, Type outType, Func<object, object> fnc)
		{
			var t = typeof(Tools);
			var mi = t.GetMethod ("ConvertFuncTT", BindingFlags.Static| BindingFlags.Public );
			var miTT =mi.MakeGenericMethod (inType, outType);
			return (Delegate)miTT.Invoke (null, new object[]{ fnc });
		}

		public static Func<Tin, Tout> ConvertFuncTT<Tin, Tout>( Func<object, object> fnc)
		{
			Func<Tin, Tout> ans	 = (o) => (Tout)fnc (o);
			return ans;
		}
    }
}
