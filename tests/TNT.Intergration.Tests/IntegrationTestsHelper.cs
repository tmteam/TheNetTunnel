using System;
using System.Collections.Generic;
using System.IO;
using CommonTestTools.Contracts;
using Moq;
using TNT.Api;
using TNT.IntegrationTests.Serialization;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace TNT.IntegrationTests;

public static class IntegrationTestsHelper
{
    public static Company CreateCompany(int usersCount)
    {
        Random rnd = new Random();
        List<User> users = new List<User>();
        for (int i = 0; i < usersCount; i++)
        {
            var usr = new User
            {
                Age = i,
                Name = "Some user with name of Masha#" + i,
                Payload = new byte[i],
            };
            rnd.NextBytes(usr.Payload);
            users.Add(usr);
        }
        var company = new Company
        {
            Name = "Microzoft",
            Id = 42,
            Users = users.ToArray()
        };
        return company;
    }
    public static PresentationBuilder<ITestContract> GetOriginBuilder()
    {
        return TntBuilder
            .UseContract<ITestContract, TestContractMock>();
    }
    public static PresentationBuilder<ITestContract> GetProxyBuilder()
    {
        return TntBuilder
            .UseContract<ITestContract>();
    }
    public static SerializationRule GetThrowsSerializationRuleFor<T>()
    {
        var fakeSerializer = new Mock<ISerializer>();

        fakeSerializer
            .Setup(s => s.Serialize(It.IsAny<object>(), It.IsAny<MemoryStream>()))
            .Callback(() => { throw new Exception("Fake exception"); });
        var throwsRule = new SerializationRule((t) => t == typeof(T), (t) => fakeSerializer.Object);
        return throwsRule;
    }

    public static DeserializationRule GetThrowsDeserializationRuleFor<T>()
    {
        var fakeDeserializer = new Mock<IDeserializer>();

        fakeDeserializer
            .Setup(s => s.Deserialize(It.IsAny<Stream>(), It.IsAny<int>()))
            .Callback(() => { throw new Exception(); });
        var throwsRule = new DeserializationRule((t) => t == typeof(T), (t) => fakeDeserializer.Object);
        return throwsRule;
    }
}