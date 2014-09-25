using System;
using ProtoBuf;
using TheTunnel;

namespace SomeContract
{
	public class ServerFtpContract
	{
		[In(1)]
		public FileList GetFileList(string path)
		{
			return null;
		}
		[In(2)]
		public bool Rename(RenameAsk ask)
		{
			return false;
		}
	}

	[ProtoContract]	public class FileList{
		[ProtoMember(1)]
		public string Path{get;set;}
		[ProtoMember(2)]
		public bool IsExist{get;set;}
		[ProtoMember(3)]
		//[ProtoInclude(1, typeof(DirrectoryBase))]
		//[ProtoInclude(2, typeof(FileInfo))]
		public DirrectoryBase[] List{get;set;}
	}
	[ProtoContract]	public class DirrectoryBase
	{
		[ProtoMember(1)] public string Path{get;set;}
		[ProtoMember(2)]public string Name{get;set;}
	}
	[ProtoContract]	public class FileInfo:DirrectoryBase
	{
		[ProtoMember(3)] public string Name;
		[ProtoMember(4)] public int Size;
		[ProtoMember(5)] public DateTime Timestamp;
	}
	[ProtoContract]	public class RenameAsk{
		[ProtoMember(1)] public string OldPath{get;set;}
		[ProtoMember(2)] public string NewName{get;set;}
	}
	
}

