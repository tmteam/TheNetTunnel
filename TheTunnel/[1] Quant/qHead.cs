using System;
using System.Runtime.InteropServices;

namespace TheTunnel
{
	[StructLayout(LayoutKind.Explicit, Size= 11)]
	public struct qHead
	{
		[FieldOffset(0)]
		public UInt16 lenght;
		[FieldOffset(2)]
		public Int32 msgId;
		[FieldOffset(6)]
		public qType type;
		[FieldOffset(7)]
		public Int32 typeArg;
	}
}

