using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoBuf;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace TNT.Core.Tests.Serialization;

[TestFixture]
public class ProtoSerializerTest
{
    [Test]
    public void SimpleType_SerailizeAndBack_ValuesAreEqual()
    {
        var value = new User
        {
            Name = "Kolya",
            Age = 24,
            IsFemale = false
        };
        using var result = new MemoryStream();
        var primitiveSerializator = new ProtoSerializer<User>();
        primitiveSerializator.SerializeT(value, result);

        result.Position = 0;

        var deserialized = new ProtoDeserializer<User>().DeserializeT(result, (int) result.Length);

        Assert.IsNotNull(deserialized);
        Assert.IsTrue(value.IsSameTo(deserialized));
    }

    [Test]
    public void ComplexType_SerailizeAndBack_ValuesAreEqual()
    {
        var value = new Team
        {
            Name = "av_team",
            Memebers = new[]
            {
                new User
                {
                    Name = "Kolya",
                    Age = 24,
                    IsFemale = false
                },
                new User
                {
                    Name = "Petya",
                    Age = 2,
                    IsFemale = false
                },
                new User
                {
                    Name = "Sasha",
                    Age = 3,
                    IsFemale = false
                }
            }
        };
        using var result = new MemoryStream();
        var primitiveSerializator = new ProtoSerializer<Team>();
        primitiveSerializator.SerializeT(value, result);

        result.Position = 0;

        var deserialized = new ProtoDeserializer<Team>().DeserializeT(result, (int) result.Length);

        Assert.IsNotNull(deserialized);
        Assert.IsTrue(value.IsSameTo(deserialized));
    }

}

[ProtoContract]
public class Team
{
    [ProtoMember(3)]
    public string Name { get; set; }
    [ProtoMember(2)]
    public User[] Memebers { get; set; }

    public bool IsSameTo(Team team)
    {
        if (Name != team.Name)
            return false;
        if (Memebers.Length != team.Memebers.Length)
            return false;
        foreach (var member in Memebers)
        {
            if(!team.Memebers.Any(m=>m.IsSameTo(member)))
                return false;
        }
        return true;

    }
}

[ProtoContract]
public class User
{
    [ProtoMember(3)] public string Name { get; set; }
    [ProtoMember(1)] public bool IsFemale { get; set; }
    [ProtoMember(2)] public int Age { get; set; }

    public bool IsSameTo(User user)
    {
        return Name == user.Name && IsFemale == user.IsFemale && Age == user.Age;
    }
}