﻿using System;
using System.Runtime.InteropServices;

namespace TheTunnel
{
	[StructLayout(LayoutKind.Explicit, Size= 7)]
	public struct QuantumHead
	{
		[FieldOffset(0)] UInt16 lenght;
		[FieldOffset(2)] Int32 msgId;
		[FieldOffset(6)] QuantumType type;
	}

	public enum QuantumType: byte{
		Start = 1,
		Data = 2,
		AbortSending = 3,
	}

}



