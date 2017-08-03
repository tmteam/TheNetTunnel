using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TNT
{
    public static class Tools
    {
        public static void SetToArray<T>(this T str, byte[] array, int offset, int size = -1)
        {
            if (size == -1)
                size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, array, offset, size);
            Marshal.FreeHGlobal(ptr);
        }

        public static T ToStruct<T>(this byte[] array, int offset, int size = -1)
        {
            if (size == -1)
                size = Marshal.SizeOf(typeof(T));
            IntPtr p = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, offset, p, size);
            T ans = (T) Marshal.PtrToStructure(p, typeof(T));
            Marshal.FreeHGlobal(p);
            return ans;
        }

        public static void WriteShort(short outputMessageId, MemoryStream to)
        {
            //Write first byte
            to.WriteByte((byte)(outputMessageId & 0xFF));
            //Write second byte
            to.WriteByte((byte)(outputMessageId >> 8));
        }

        public static short? TryReadShort(this MemoryStream from)
        {
            if (@from.Length - @from.Position < sizeof(short))
                return null;
            return @from.ReadShort();
        }

        public static bool TryReadShort(this MemoryStream from, out short value)
        {
            value = 0;
            if (@from.Length - @from.Position < sizeof(short))
                return false;
            value = @from.ReadShort();
            return true;
        }

        public static short ReadShort(this MemoryStream from)
        {
            if (@from.Length - @from.Position < 2)
                throw new EndOfStreamException();
            byte[] arr = new byte[2];
            @from.Read(arr, 0, 2);

            return BitConverter.ToInt16(arr, 0);
        }

        public static void CopyToAnotherStream(this Stream stream, Stream targetStream, int lenght)
        {
            int lasts = lenght;
            while (lasts > 0)
            {
                byte[] arr = new byte[lasts > 4096 ? 4096 : lasts];
                stream.Read(arr, 0, arr.Length);
                targetStream.Write(arr, 0, arr.Length);
                lasts -= arr.Length;
            }
        }

        public static void WriteToStream<T>(this T str, Stream stream, int size = -1)
        {
            if (size == -1)
                size = Marshal.SizeOf(str);
            var arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            stream.Write(arr, 0, size);
        }
    }
}
