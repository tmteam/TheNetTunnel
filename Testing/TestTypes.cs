using System;
using ProtoBuf;
using System.Linq;
using System.Runtime.InteropServices;

namespace A3Expit
{
	public enum testEn:byte{
		first = 1,
		second = 2,
		third = 3,
	}

	[ProtoContract]
	public class ToiletType
	{
		public static ToiletType GetRandomType()
		{
			var tc = Tools.rnd.Next () % 100;
			return new ToiletType {
				ToiletsNames = Enumerable.Range (0, tc+1).Select (r => "toilet#"+r).ToArray (),
				ReleaseTime = DateTime.Now + TimeSpan.FromMilliseconds (Tools.rnd.Next () % 1024),
				ToiletTypeName = "type#" + Tools.rnd.Next ().ToString (),
			};
		}

		[ProtoMember(1)] public string[] ToiletsNames{get;set;}
		[ProtoMember(2)] public string ToiletTypeName { get; set; }
		[ProtoMember(3)] public DateTime ReleaseTime{ get; set; }

		public bool IsEqual(ToiletType otherType)
		{
			if (ToiletsNames == null)
				return otherType.ToiletsNames == null;
			if (otherType.ToiletsNames == null)
				return false;

			for (int i = 0; i < ToiletsNames.Length; i++)
				if (ToiletsNames [i].CompareTo (otherType.ToiletsNames [i]) != 0)
					return false;

			return  string.Compare (ToiletTypeName, otherType.ToiletTypeName) == 0 && ReleaseTime.CompareTo (otherType.ReleaseTime) == 0;
		}
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1, Size= 21)]//OCHKO!
	public struct Toilet{

		public static Toilet FindClosestToilet()
		{
			return new Toilet
			{
				Volume = 100 * Tools.rnd.NextDouble (),
				InUse = (byte)(Tools.rnd.Next() %2),
				TimesUsed = (uint)Tools.rnd.Next(),
				LastUsedTime = Tools.rnd.Next(),
			};
		}

		[FieldOffset(0)]  public double Volume;
		[FieldOffset(8)]  public byte InUse;// U should not to use (bool) type! it got Marshaling deserialization problem.
		[FieldOffset(9)]  public UInt32 TimesUsed;
		[FieldOffset(13)] public long LastUsedTime;// U should not to use (DateTime) type. Its impolssinle to serialize this type by usual Marshaling.

		public bool IsEqual(Toilet toil)
		{
			return toil.InUse == InUse && toil.Volume == Volume && toil.LastUsedTime == LastUsedTime && toil.TimesUsed == TimesUsed;
		}

		public static bool AreEqual(Toilet[] A, Toilet[] B)
		{
			if (A == null)
				return B == null;
			if (B == null)
				return false;

			if (A.Length != B.Length)
				return false;
			for (int i = 0; i < A.Length; i++)
				if (!A [i].IsEqual (B [i]))
					return false;
			return true;
		}
	}
	[ProtoBuf.ProtoContract]
	public class ProtoPoint{ 
		[ProtoBuf.ProtoMember(1)] public int X;
		[ProtoBuf.ProtoMember(2)] public int Y;
	}
}

