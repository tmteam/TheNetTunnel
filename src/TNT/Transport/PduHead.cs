using System;
using System.Runtime.InteropServices;

namespace TNT.Transport
{
	[StructLayout(LayoutKind.Explicit, Size= 7)]
	public struct PduHead
	{
        public static readonly int DefaultHeadSize = Marshal.SizeOf(typeof(PduHead));

        /// <summary>
        /// Full quantum lenght
        /// </summary>
		[FieldOffset(0)] public UInt16 length;
        /// <summary>
        /// Light message ID
        /// </summary>
		[FieldOffset(2)] public Int32 msgId;
        /// <summary>
        /// Type of a quant
        /// </summary>
		[FieldOffset(6)] public PduType type;
	}
}



