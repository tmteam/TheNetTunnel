using System;
using System.Runtime.InteropServices;

namespace TNT.Light
{
	[StructLayout(LayoutKind.Explicit, Size= 7)]
	public struct QuantumHead
	{
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
		[FieldOffset(6)] public QuantumType type;
	}

	public enum QuantumType: byte{
		Start = 1,
		Data = 2,
		AbortSending = 3,
	}

}



