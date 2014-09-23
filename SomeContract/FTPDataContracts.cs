using System;
using ProtoBuf;

namespace SomeContract
{
	[ProtoContract]
	public class FileInfo
	{
		[ProtoMember(1)] public string Name;
		[ProtoMember(2)] public int Size;
		[ProtoMember(3)] public DateTime Timestamp;
	}
}

