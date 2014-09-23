using System;
using ProtoBuf;

namespace SomeContract
{
	[ProtoContract]
	public class UserInfo
	{	[ProtoMember(1)] public string FullName;
		[ProtoMember(2)] public string Nick;
	}
	[ProtoContract]
	public class RegistrationResults
	{
		[ProtoMember(1)] public bool Result;
		[ProtoMember(2)] public DateTime TimeStamp;
	}
	[ProtoContract]
	public class Msg
	{	[ProtoMember(1)] public UserInfo User;
		[ProtoMember(2)] public string Message;
		[ProtoMember(3)] public DateTime Timestamp;
	}
	[ProtoContract]
	public class DisconnectInfo
	{	[ProtoMember(1)] public UserInfo Initiator;
		[ProtoMember(2)] public DisconnectReason Reason;
		[ProtoMember(3)] public string Message;
		[ProtoMember(4)] public DateTime Timestamp;
	}

	public enum DisconnectReason
	{
		Kick,
		AsWill,
	}

}

