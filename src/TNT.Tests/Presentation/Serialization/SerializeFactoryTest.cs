using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using TNT.Exceptions.ContractImplementation;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace TNT.Tests.Presentation.Serialization
{
    [TestFixture]
    public class SerializeFactoryTest
    {
        [Test]
        public void EmptySerializeFactory_Create_Throws()
        {
            var factory = new SerializerFactory();
            Assert.Throws<TypeCannotBeSerializedException>(() =>  factory.Create(typeof(int)));
        }

        [Test]
        public void EmptyDeserializeFactory_Create_Throws()
        {
            var factory = new DeserializerFactory();
            Assert.Throws<TypeCannotBeDeserializedException>(() => factory.Create(typeof(int)));
        }
        
        [TestCase(typeof(int),            typeof(ValueTypeSerializer<int>))]
        [TestCase(typeof(DateTime),       typeof(UTCFileTimeSerializer))]
        [TestCase(typeof(DateTimeOffset), typeof(UTCFileTimeAndOffsetSerializer))]
        [TestCase(typeof(int[]),          typeof(ArraySerializer<int[]>))]
        [TestCase(typeof(string[]),       typeof(ArraySerializer<string[]>))]
        [TestCase(typeof(string),         typeof(UnicodeSerializer))]
        [TestCase(typeof(string),         typeof(UnicodeSerializer))]
        [TestCase(typeof(MyStructStub),   typeof(ValueTypeSerializer<MyStructStub>))]
        [TestCase(typeof(User),           typeof(ProtoSerializer<User>))]
        [TestCase(typeof(MyEnumDefType), typeof(EnumSerializer<MyEnumDefType>))]
        [TestCase(typeof(MyEnumByteType), typeof(EnumSerializer<MyEnumByteType>))]
        [TestCase(typeof(MyEnumLongType), typeof(EnumSerializer<MyEnumLongType>))]
        [TestCase(typeof(MyEnumULongType), typeof(EnumSerializer<MyEnumULongType>))]
        [TestCase(typeof(MyEnumintType), typeof(EnumSerializer<MyEnumintType>))]
        public void DefaultSerializerFactory_CreateForType_ReturnsCorrectSerializer(Type serializeable, Type serializerType)
        {
            var factory = SerializerFactory.CreateDefault();
            var serializer = factory.Create(serializeable);
            Assert.IsInstanceOf(serializerType,serializer);
        }
        
        [TestCase(typeof(int),            typeof(ValueTypeDeserializer<int>))]
        [TestCase(typeof(DateTime),       typeof(UTCFileTimeDeserializer))]
        [TestCase(typeof(DateTimeOffset), typeof(UTCFileTimeAndOffsetDeserializer))]
        [TestCase(typeof(int[]),          typeof(ArrayDeserializer<int[]>))]
        [TestCase(typeof(string[]),       typeof(ArrayDeserializer<string[]>))]
        [TestCase(typeof(string),         typeof(UnicodeDeserializer))]
        [TestCase(typeof(string),         typeof(UnicodeDeserializer))]
        [TestCase(typeof(MyStructStub),   typeof(ValueTypeDeserializer<MyStructStub>))]
        [TestCase(typeof(User),           typeof(ProtoDeserializer<User>))]
        [TestCase(typeof(MyEnumDefType),  typeof(EnumDeserializer<MyEnumDefType>))]
        [TestCase(typeof(MyEnumByteType), typeof(EnumDeserializer<MyEnumByteType>))]
        [TestCase(typeof(MyEnumLongType), typeof(EnumDeserializer<MyEnumLongType>))]
        [TestCase(typeof(MyEnumULongType), typeof(EnumDeserializer<MyEnumULongType>))]
        [TestCase(typeof(MyEnumintType),   typeof(EnumDeserializer<MyEnumintType>))]
        public void DefaultDeserializerFactory_CreateForType_ReturnsCorrectDeserializer(Type Deserializeable, Type DeserializerType)
        {
            var factory = DeserializerFactory.CreateDefault();
            var Deserializer = factory.Create(Deserializeable);
            Assert.IsInstanceOf(DeserializerType, Deserializer);
        }
        [TestCase(42)]
        [TestCase(42.0)]
        [TestCase(42.0f)]
        [TestCase(42l)]
        [TestCase(true)]
        [TestCase(false)]
        [TestCase(MyEnumDefType.One)]
        [TestCase(MyEnumDefType.Two)]
        [TestCase(MyEnumintType.One)]
        [TestCase(MyEnumintType.Two)]
        [TestCase(MyEnumByteType.One)]
        [TestCase(MyEnumByteType.Two)]
        [TestCase(MyEnumLongType.One)]
        [TestCase(MyEnumLongType.Two)]
        [TestCase(MyEnumLongType.Min)]
        [TestCase(MyEnumLongType.Max)]
        [TestCase(MyEnumULongType.One)]
        [TestCase(MyEnumULongType.Two)]
        [TestCase(MyEnumULongType.Max)]
        [TestCase('i')]
        [TestCase("")]
        public void SerializeAndDeserializeBack_OriginAndDeserializedAreEqual(object val)
        {
            object deserialized = SerializeAndDeserializeBack(val);
            Assert.AreEqual(val, deserialized);
        }

        [Test]
        public void ProtoContract_SerializeAndDeserializeBack_OriginAndDeserializedAreEqual()
        {
            var origin = new Team()
            {
                Name = "avTeam",
                Memebers = new []
                {
                    new User
                    {
                        Age = 24,
                        IsFemale = true,
                        Name = "Kate"
                    }
                }
            };
          
            var deserialized = SerializeAndDeserializeBack(origin);
            Assert.IsTrue(origin.IsSameTo((Team)deserialized));
        }

        [Test]
        public void NullableValuableInt_OriginAndDeserializedAreEqual()
        {
            int? origin = 42;
            var deserialized = SerializeAndDeserializeBack(origin);
            Assert.AreEqual(deserialized, origin);
        }
        [TestCase(42)]
        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestCase(int.MinValue)]
        [TestCase(null)]
        public void NullableIntNull_OriginAndDeserializedAreEqual(int? value)
        {
            var deserialized = SerializeAndDeserializeBack(value);
            Assert.AreEqual(deserialized, value);
        }
        [Test]
        public void ArrayOfStrings_SerializeAndDeserializeBack_OriginAndDeserializedAreEqual()
        {
            var emptyString = string.IsInterned("");
            var origin = new[] { "lalala", emptyString, "bububu", null};
            var deserialized = (string[]) SerializeAndDeserializeBack(origin);
            CollectionAssert.AreEqual(origin, deserialized);
        }
        [Test]
        public void ArrayOfInt_SerializeAndDeserializeBack_OriginAndDeserializedAreEqual()
        {
            var origin = new[] { 1, 2, 3, 4, 5 };
            var deserialized = (int[])SerializeAndDeserializeBack(origin);
            CollectionAssert.AreEqual(origin, deserialized);
        }
        [Test]
        public void ArrayOfBytes_SerializeAndDeserializeBack_OriginAndDeserializedAreEqual()
        {
            var origin = new byte[] { 1, 2, 3, 4, 5 };
            var deserialized = (byte[])SerializeAndDeserializeBack(origin);
            CollectionAssert.AreEqual(origin, deserialized);
        }
        [Test]
        public void EmptyArrayOflong_SerializeAndDeserializeBack_OriginAndDeserializedAreEqual()
        {
            var origin = new long[0];
            var deserialized = (long[])SerializeAndDeserializeBack(origin);
            CollectionAssert.AreEqual(origin, deserialized);
        }
        [Test]
        public void EmptyArrayOfStrings_SerializeAndDeserializeBack_OriginAndDeserializedAreEqual()
        {
            var origin = new string[0];
            var deserialized = (string[])SerializeAndDeserializeBack(origin);
            CollectionAssert.AreEqual(origin, deserialized);
        }
        [Test]
        public void Struct_SerializeAndDeserializeBack_OriginAndDeserializedAreEqual()
        {
            var origin = new MyStructStub {
                theProperty = 42
            };
        
            var deserialized =  (MyStructStub)SerializeAndDeserializeBack(origin);
            Assert.AreEqual(origin.theProperty, deserialized.theProperty);
        }


        [Test]
        public void PrimitiveTypesSequence_SerializeAndDeserializeBack_OriginAndDeserializedAreEqual()
        {
            var origin = new object[]
            {
                "fortyTwo",
                42,
                42d,
                MyEnumDefType.Two,
                MyEnumULongType.Max
            };
            var sequenceTypes = origin.Select(o => o.GetType()).ToArray();

            var serializer = SerializerFactory.CreateDefault().Create(sequenceTypes);
            var deserializer = DeserializerFactory.CreateDefault().Create(sequenceTypes);
            var stream = new MemoryStream();
            serializer.Serialize(origin, stream);
            stream.Position = 0;
            var deserialized = (object[])deserializer.Deserialize(stream, (int)stream.Length);
            CollectionAssert.AreEqual(origin,deserialized);
        }

        [Test]
        public void ComplexTypesSequence_SerializeAndDeserializeBack_OriginAndDeserializedAreEqual()
        {
            var origin = new object[]
            {
                "fortyTwo",
                "",
                new int[] {1, 2, 3, 4},
                new User
                {
                    Age = 18,
                    IsFemale = true,
                    Name = "Kate"
                },
                new MyStruct()
                {
                    MyBool = true,
                    MyDate = DateTime.Now,
                    MyInt = 42,
                    MyLong = 42,
                }
            };

            var sequenceTypes = origin.Select(o => o.GetType()).ToArray();

            var serializer = SerializerFactory.CreateDefault().Create(sequenceTypes);
            var deserializer = DeserializerFactory.CreateDefault().Create(sequenceTypes);
            var stream = new MemoryStream();
            serializer.Serialize(origin, stream);
            stream.Position = 0;
            var deserialized = (object[])deserializer.Deserialize(stream, (int)stream.Length);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(origin[0], deserialized[0]);
                Assert.AreEqual(origin[1], deserialized[1]);
                CollectionAssert.AreEqual((IEnumerable) origin[2], (IEnumerable) deserialized[2]);
                Assert.IsTrue(((User) origin[3]).IsSameTo((User) deserialized[3]));
                Assert.IsTrue(((MyStruct) origin[4]).IsSameTo((MyStruct) deserialized[4]));
                Assert.AreEqual(origin.Length, deserialized.Length);
            });
        }

        private static T SerializeAndDeserializeBack<T>(T origin)
        {
            var type = typeof(T);
            var serializer = SerializerFactory.CreateDefault().Create(type);
            var deserializer = DeserializerFactory.CreateDefault().Create(type);
            var stream = new MemoryStream();
            serializer.Serialize(origin, stream);
            stream.Position = 0;
            var deserialized = (T)deserializer.Deserialize(stream, (int)stream.Length);
            return deserialized;
        }
        private static object SerializeAndDeserializeBack(object origin)
        {
            var serializer = SerializerFactory.CreateDefault().Create(origin.GetType());
            var deserializer = DeserializerFactory.CreateDefault().Create(origin.GetType());
            var stream = new MemoryStream();
            serializer.Serialize(origin, stream);
            stream.Position = 0;
            var deserialized = deserializer.Deserialize(stream, (int)stream.Length);
            return deserialized;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct MyStructStub
        {
            [FieldOffset(0)] public int theProperty;
        }

        enum MyEnumDefType
        {
            One,
            Two
        }

        enum MyEnumintType: int
        {
            One = 0,
            Two = 1,
        }

        enum MyEnumByteType: byte
        {
            One = 0,
            Two = 2,
        }

        enum MyEnumLongType : long
        {
            Min = long.MinValue +1,
            One = 1,
            Two = 2,
            Max = long.MaxValue-1,
        }

        enum MyEnumULongType : ulong
        {
            One = 1,
            Two = 2,
            Max = ulong.MaxValue - 1,
        }
    }
}
