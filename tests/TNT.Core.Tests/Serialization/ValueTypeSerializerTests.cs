using System;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace TNT.Core.Tests.Serialization;

[TestFixture]
public class PrimitiveSerializerTests
{
    [TestCase(3452341.12)]
    [TestCase(12)]
    [TestCase(0)]
    [TestCase(0.001)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Double_SerializeAndBack_ValuesAreEquel(double value)
    {
        Assert.AreEqual(value,SerializeAndBack(value));
    }

        

    [TestCase(true)]
    [TestCase(false)]
    public void Bool_SerializeAndBack_ValuesAreEquel(bool value)
    {
        Assert.AreEqual(value, SerializeAndBack(value));
    }
       
    [Test]
    public void MyStruct_SerializeAndBack_ValuesAreEquel()
    {
        var myStruct = new MyStruct { MyBool = true, MyInt = 5545, MyLong = 54454, MyDate = DateTime.Now};
        Assert.IsTrue(myStruct.IsSameTo(SerializeAndBack(myStruct)));
    }


    private static T SerializeAndBack<T>(T value) where T : struct
    {
        using var result = new MemoryStream();
        var primitiveSerializator = new ValueTypeSerializer<T>();
        primitiveSerializator.SerializeT(value, result);

        result.Position = 0;

        return new ValueTypeDeserializer<T>().DeserializeT(result, (int)primitiveSerializator.Size);
    }

}

[StructLayout(layoutKind: LayoutKind.Explicit)]
public struct MyStruct
{
    [FieldOffset(0)] public int MyInt;

    [FieldOffset(4)] public bool MyBool;

    [FieldOffset(5)] public long MyLong;

    [FieldOffset(13)] public DateTime MyDate;

    public override string ToString()
    {
        return string.Format("MyInt - {0} MyBool - {1} MyLong - {2} MyDate - {3}", MyInt, MyBool, MyLong, MyDate);
    }

    public bool IsSameTo(MyStruct other)
    {
        return MyInt == other.MyInt 
               && MyBool == other.MyBool 
               && MyLong == other.MyLong 
               && 
               Math.Abs((MyDate - other.MyDate).TotalMilliseconds)<1000;
    }
}