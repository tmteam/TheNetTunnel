using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System.Runtime.InteropServices;
using ProtoBuf;

namespace Try
{
	[TestFixture]
	public class De_Serialize
	{
		static De_Serialize()
		{
			rnd = new Random(DateTime.Now.Millisecond);
		}

		public static Random rnd{get; private set;}

		[Test]	public void Unicode_De_serialization()
		{
			string test = @"suppose I should be upset, even feel violated, but I'm not. No, in fact, I think this is a friendly message, like ""Hey, wanna play?"" and yes, I want to play. I really, really do. ";
			var ser = TheTunnel.SerializersFactory.GetSerializer (typeof(string));
			var bytestring = ser.Serialize (test, 5);
			List<byte> lb = new List<byte> (bytestring);
			lb.Add (12);
			lb.Add (0);
			lb.Add (0);
			bytestring = lb.ToArray ();
			var deser = TheTunnel.SerializersFactory.GetDeserializer (typeof(string));

			object ostrres;
			if (!deser.TryDeserialize (bytestring, 5, out ostrres))
				throw new Exception ("Deserialization failed");
			if (String.Compare (test, (string)ostrres) != 0)
				throw new Exception ("original and deserizlized strings are not equal");

		}

		[Test]	public void DateTime_De_serialization()
		{
			DateTime tt = DateTime.Now;

			var ser = TheTunnel.SerializersFactory.GetSerializer (typeof(DateTime));
			var deser = TheTunnel.SerializersFactory.GetDeserializer (typeof(DateTime));

			var stream = ser.Serialize (tt,5);

			object res;
			if (!deser.TryDeserialize (stream, 5, out res))
				throw new Exception ("Deserialization failed");

			if (DateTime.Compare (tt, (DateTime)res) != 0) {
				throw new Exception ("original and deserizlized timestamps are not equal");
			}


		}

		[Test] public void ProtoBuf_DeSerialization()
		{
			var tt = ToiletType.GetRandomType ();
			var ser = TheTunnel.SerializersFactory.GetSerializer (tt.GetType ());
			var deser = TheTunnel.SerializersFactory.GetDeserializer (tt.GetType ());
			var bt = ser.Serialize (tt, 7);

			object res;
			if(!deser.TryDeserialize(bt, 7, out res))
				throw new Exception ("Deserialization failed");

			var dett = res as ToiletType;

			if(!dett.IsEqual(tt))
				throw new Exception ("original and deserializerd object are not equal");
		}

		[Test]	public void FixedArray_DeSerialization()
		{
			int tc = 1000;

			Toilet[] wcs = new Toilet[tc];

			for (int i = 0; i < tc; i++)
				wcs [i] = Toilet.FindClosestToilet ();

			var ser = TheTunnel.SerializersFactory.GetSerializer (wcs.GetType ());
			var deser = TheTunnel.SerializersFactory.GetDeserializer (wcs.GetType ());

			var bt = ser.Serialize (wcs, 7);

			object res;
			if(!deser.TryDeserialize(bt, 7, out res))
				throw new Exception ("Deserialization failed");

			var detoils = res as Toilet[];

			if(detoils==null)
				throw new Exception ("deserialized object has wrong type");
				
			if(detoils.Length!= wcs.Length)
				throw new Exception ("original and deserializerd toilet array lenght are not equal");
				
			for(int i = 0; i<tc; i++)
				if(!detoils[i].IsEqual(wcs[i]))
					throw new Exception ("original and deserializerd toilet["+i+"] are not equal");
		}

		[Test] public void UnicodeArray_DeSerialization(){
			var ttar = Enumerable.Range (0, 100).Select (r => "string#"+r).ToArray ();
			var ser = TheTunnel.SerializersFactory.GetSerializer (ttar.GetType ());
			var deser = TheTunnel.SerializersFactory.GetDeserializer (ttar.GetType ());
			var bt = 	ser.Serialize (ttar, 7);

			object res;
			if(!deser.TryDeserialize(bt, 7, out res))
				throw new Exception ("Deserialization failed");

			var dett = res as string[];

			if(dett.Length!= ttar.Length)
				throw new Exception ("original and deserialized arrays lenght are not equal");

			for(int i = 0; i<dett.Length; i++)
				if(dett[i].CompareTo(ttar[i])!=0)
					throw new Exception ("original and deserializerd object are not equal");
		}

		[Test]  public void ProtoArray_DeSerialize(){
			var ttar = Enumerable.Range (0, 100).Select (r => ToiletType.GetRandomType ()).ToArray ();
			var ser = TheTunnel.SerializersFactory.GetSerializer (ttar.GetType ());
			var deser = TheTunnel.SerializersFactory.GetDeserializer (ttar.GetType ());
			var bt = 	ser.Serialize (ttar, 7);

			object res;
			if(!deser.TryDeserialize(bt, 7, out res))
				throw new Exception ("Deserialization failed");

			var dett = res as ToiletType[];

			for(int i = 0; i<dett.Length; i++)
				if(!dett[i].IsEqual(ttar[i]))
					throw new Exception ("original and deserializerd object are not equal");
		}
	}

	[ProtoContract]
	public class ToiletType
	{
		public static ToiletType GetRandomType()
		{
			var tc = De_Serialize.rnd.Next () % 100;
			return new ToiletType {
				ToiletsNames = Enumerable.Range (0, tc+1).Select (r => "toilet#"+r).ToArray (),
				ReleaseTime = DateTime.Now + TimeSpan.FromMilliseconds (De_Serialize.rnd.Next () % 1024),
				ToiletTypeName = "type#" + De_Serialize.rnd.Next ().ToString (),
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
				Volume = 100 * De_Serialize.rnd.NextDouble (),
				InUse = (byte)(De_Serialize.rnd.Next() %2),
				TimesUsed = (uint)De_Serialize.rnd.Next(),
				LastUsedTime = De_Serialize.rnd.Next(),
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
}

