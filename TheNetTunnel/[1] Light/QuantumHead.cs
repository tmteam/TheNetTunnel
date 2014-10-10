using System;
using System.Runtime.InteropServices;

namespace TheTunnel.Light
{
	[StructLayout(LayoutKind.Explicit, Size= 7)]
	public struct QuantumHead
	{
		[FieldOffset(0)] public UInt16 length;
		[FieldOffset(2)] public Int32 msgId;
		[FieldOffset(6)] public QuantumType type;
	}

	public enum QuantumType: byte{
		Start = 1,
		Data = 2,
		AbortSending = 3,
	}

}



