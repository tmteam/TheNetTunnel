using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System.Runtime.InteropServices;
using ProtoBuf;
using TheTunnel;

namespace Try
{
	[TestFixture]  public class De_Serialize
	{
		static De_Serialize() { rnd = new Random (DateTime.Now.Millisecond);}

		public static Random rnd{get; private set;}

		public static T CheckAndRecreate<T>(T origin)
		{
			int offs = 7;

			var ser = TheTunnel.SerializersFactory.Create (typeof(T));
			var deser = TheTunnel.DeserializersFactory.Create (typeof(T));

			var stream = ser.Serialize (origin,offs);
			byte[] trystream;

			if (!ser.TrySerialize (origin, offs, out trystream))
				throw new Exception ("TrySerialize(out object) failure");
			if (!stream.SequenceEqual (trystream))
				throw new Exception ("Serialize and TrySerialize(out object) results are not equal");

			var serT = ser as ISerializer<T>;
			if (serT != null) {
				if (!serT.TrySerialize (origin, trystream, offs))
					throw new Exception ("TrySerialize failure");
				if (!stream.SequenceEqual (trystream))
					throw new Exception ("Serialize and TrySerialize results are not equal");
			}


			object res;
			if (!deser.TryDeserialize (stream, offs, out res))
				throw new Exception ("Deserialization(object) failed");

			T deserialized;
			if(!(deser as IDeserializer<T>).TryDeserializeT(stream,offs, out deserialized))
				throw new Exception ("DeserializationT failed");
				
			return deserialized;

		}

		[Test]	public void Unicode()
		{
			string origin = @"suppose I should be upset, even feel violated, but I'm not. No, in fact, I think this is a friendly message, like ""Hey, wanna play?"" and yes, I want to play. I really, really do. ";

			var deserialized = CheckAndRecreate (origin);

			if (String.Compare (origin, origin) != 0)
				throw new Exception ("original and deserizlized strings are not equal");
		}

		[Test]	public void UTCFileTime()
		{
			DateTime origin = DateTime.Now;

			var deserialized = CheckAndRecreate (origin);

			if (DateTime.Compare (origin, deserialized)!= 0) {
				throw new Exception ("original and deserizlized timestamps are not equal");
			}


		}

		[Test]  public void ProtoBuf()
		{
			var origin = ToiletType.GetRandomType ();

			var deserialized = CheckAndRecreate (origin);

			if(!deserialized.IsEqual(origin))
				throw new Exception ("original and deserializerd object are not equal");
		}

		[Test]  public void ByteArray()
		{
			byte[] origin = new byte[10000];
			rnd.NextBytes (origin);

			var deserialized = CheckAndRecreate (origin);

			if(!origin.SequenceEqual(deserialized))
				throw new Exception ("original and deserizlized arrays are not equal");
		}

		[Test]	public void FixedArray()
		{
			int tc = 1000;
			Toilet[] origin = new Toilet[tc];

			for (int i = 0; i < tc; i++)
				origin [i] = Toilet.FindClosestToilet ();

			var deserialized = CheckAndRecreate (origin);
				
			for(int i = 0; i<tc; i++)
				if(!deserialized[i].IsEqual(origin[i]))
					throw new Exception ("original and deserializerd toilet["+i+"] are not equal");
		}

		[Test]  public void UnicodeArray()
		{
			var origin = Enumerable.Range (0, 100).Select (r => "string#"+r).ToArray ();

			var deserialized = CheckAndRecreate (origin);

			for(int i = 0; i<deserialized.Length; i++)
				if(deserialized[i].CompareTo(origin[i])!=0)
					throw new Exception ("original and deserializerd object are not equal");
		}

		[Test]  public void ProtoArray()
		{
			var origin = Enumerable.Range (0, 100).Select (r => ToiletType.GetRandomType ()).ToArray ();

			var deserialized = CheckAndRecreate (origin);

			for(int i = 0; i<deserialized.Length; i++)
				if(!deserialized[i].IsEqual(origin[i]))
					throw new Exception ("original and deserializerd object are not equal");
		}

		[Test]  public void Sequence()
		{
			var origin = new TestSequence ();

			var ser = new SequenceSerializer (origin.SequenceTypes);
			var des = new SequenceDeserializer (origin.SequenceTypes);

			var msg1 = ser.Serialize (origin.Sequence,7);
			byte[] msg2;
			ser.TrySerialize(origin.Sequence, 7, out msg2);

			if (!msg1.SequenceEqual (msg2))
				throw new Exception ("Serialize and TrySerialize results are not equal");

			object[] deSequnce;

			if (!des.TryDeserializeT (msg1, 7, out deSequnce))
				throw new Exception ("Deserialize Failure");

			var deserialized = new TestSequence();
			deserialized.Sequence = deSequnce;

			if (!deserialized.IsEqualTo (origin))
				throw new Exception ("original and deserialized sequences are not equal");
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

	public class TestSequence{

		public int i = De_Serialize.rnd.Next();

		public ToiletType toiletType = ToiletType.GetRandomType();

		public double d = De_Serialize.rnd.NextDouble();

		public string s = "str#"+De_Serialize.rnd.Next().ToString();

		public DateTime t = DateTime.Now;

		public Toilet toilet = Toilet.FindClosestToilet();

		public Type[] SequenceTypes
		{
			get{ return Sequence.Select (s => s.GetType()).ToArray (); }
		}

		public object[] Sequence
		{
			get{ return new object[]{ i, toiletType, d, s, t, toilet };}
			set{
				if (value.Length != Sequence.Length)
					throw new Exception ("Wrong input sequence lenght");
				if (!value.Select (v => v.GetType ()).SequenceEqual (SequenceTypes))
					throw new Exception ("wrong input types sequence");
				i = (int)value [0];
				toiletType = (ToiletType)value [1];
				d = (double)value [2];
				s = (string)value [3];
				t = (DateTime)value [4];
				toilet = (Toilet)value [5];
			}
		}
		public bool IsEqualTo(TestSequence ts)
		{
			return i == ts.i && d == ts.d && t.CompareTo (ts.t) == 0 && s.CompareTo (ts.s) == 0 && toilet.IsEqual (ts.toilet) && toiletType.IsEqual (ts.toiletType);
		}
	}
}

