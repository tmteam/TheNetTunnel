using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TheTunnel
{
    public static class Tools
    {
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
    }
}
