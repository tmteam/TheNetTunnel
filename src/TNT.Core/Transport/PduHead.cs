using System;
using System.Runtime.InteropServices;

namespace TNT.Transport;

[StructLayout(LayoutKind.Explicit, Size= 9)]
public struct PduHead
{
	public static readonly int DefaultHeadSize = Marshal.SizeOf(typeof(PduHead));

	/// <summary>
	/// Full Pdu lenght
	/// </summary>
	[FieldOffset(0)] public Int32 length;
	/// <summary>
	/// Pdu message ID
	/// </summary>
	[FieldOffset(4)] public Int32 msgId;
	/// <summary>
	/// Type of the pdu message
	/// </summary>
	[FieldOffset(8)] public PduType type;
}