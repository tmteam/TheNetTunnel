using System;
using System.IO;
using CommonTestTools.Contracts;
using NUnit.Framework;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace Tnt.LongTests.TcpLocalhostSpecificTest;

[TestFixture]
public class SerializationExceptionsTest
{
    [Test]
    public void LocalProxySerializationFails_throws()
    {
        var proxyContractBuilder = IntegrationTestsHelper.GetProxyBuilder()
            .UseSerializer(IntegrationTestsHelper.GetThrowsSerializationRuleFor<string>());

        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
            (IntegrationTestsHelper.GetOriginBuilder(), proxyContractBuilder);
        //local string serializer throws.
        Assert.Multiple(() =>
        {
            Assert.Throws<LocalSerializationException>(() => tcpPair.ProxyContract.Say("testString"));
            tcpPair.AssertPairIsDisconnected();
        });
    }

    [Test]
    public void LocalOriginSerializationFails_throws()
    {
        var originContractBuilder = IntegrationTestsHelper.GetOriginBuilder()
            .UseSerializer(IntegrationTestsHelper.GetThrowsSerializationRuleFor<string>());

        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
            (originContractBuilder, IntegrationTestsHelper.GetProxyBuilder());
        //local origin serializer fails
        Assert.Multiple(() =>
        {
            Assert.Throws<LocalSerializationException>(() => tcpPair.OriginContract.OnSayS("testString"));
            tcpPair.AssertPairIsDisconnected();
        });
    }

    [Test]
    public void RemoteProxySeserializationFails_throws()
    {
        var originContractBuilder = IntegrationTestsHelper
            .GetOriginBuilder()
            .UseSerializer(IntegrationTestsHelper.GetThrowsSerializationRuleFor<string>());

        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
            (originContractBuilder, IntegrationTestsHelper.GetProxyBuilder());
        //origin contract uses failed string serializer, when it returns answer
        Assert.Multiple(() =>
        {
            Assert.Throws<RemoteSerializationException>(() => tcpPair.ProxyContract.Ask("testString"));
            tcpPair.AssertPairIsDisconnected();
        });
    }

    [Test]
    public void RemoteOriginSeserializationFails_throws()
    {
        var proxyContractBuilder = IntegrationTestsHelper
            .GetProxyBuilder()
            .UseSerializer(IntegrationTestsHelper.GetThrowsSerializationRuleFor<string>());

        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
            (IntegrationTestsHelper.GetOriginBuilder(), proxyContractBuilder);
        //proxy contract uses failed string serializer, when it returns answer
        Assert.Multiple(() =>
        {
            Assert.Throws<RemoteSerializationException>(() => tcpPair.OriginContract.OnAskS("testString"));
            tcpPair.AssertPairIsDisconnected();
        });
    }

    [Test]
    public void LocalProxyDeserializationFails_throws()
    {
        var proxyContractBuilder = IntegrationTestsHelper
            .GetProxyBuilder()
            .UseDeserializer(IntegrationTestsHelper.GetThrowsDeserializationRuleFor<string>());

        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
            (IntegrationTestsHelper.GetOriginBuilder(), proxyContractBuilder);
        //proxy contract uses failed string deserializer, when it accepts answer
        Assert.Multiple(() =>
        {
            Assert.Throws<LocalSerializationException>(() => tcpPair.ProxyContract.Ask("testString"));
            tcpPair.AssertPairIsDisconnected();
        });
    }

    [Test]
    public void LocalOriginDeserializationFails_throws()
    {
        var originContractBuilder = IntegrationTestsHelper
            .GetOriginBuilder()
            .UseDeserializer(IntegrationTestsHelper.GetThrowsDeserializationRuleFor<string>());

        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
            (originContractBuilder, IntegrationTestsHelper.GetProxyBuilder());
        tcpPair.ProxyContract.OnAskS += s => s;
        //origin contract uses failed string deserializer, when it accepts answer
        Assert.Multiple(() =>
        {
            Assert.Throws<LocalSerializationException>(() => tcpPair.OriginContract.OnAskS("testString"));
            tcpPair.AssertPairIsDisconnected();
        });
    }

    [Test]
    public void RemoteProxyDeseserializationFails_throws()
    {
        var originContractBuilder = IntegrationTestsHelper
            .GetOriginBuilder().UseDeserializer(
                IntegrationTestsHelper.GetThrowsDeserializationRuleFor<string>());

        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
            (originContractBuilder, IntegrationTestsHelper.GetProxyBuilder());
        //origin contract uses failed string deserializer, when it returns answer
        Assert.Multiple(() =>
        {
            Assert.Throws<RemoteSerializationException>(() => tcpPair.ProxyContract.Ask("testString"));
            tcpPair.AssertPairIsDisconnected();
        });
    }

     
    [Test]
    public void RemoteOriginDeserializationFails_throws()
    {
        var proxyContractBuilder = IntegrationTestsHelper
            .GetProxyBuilder()
            .UseDeserializer(IntegrationTestsHelper.GetThrowsDeserializationRuleFor<string>());

        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
            (IntegrationTestsHelper.GetOriginBuilder(), proxyContractBuilder);
        //proxy contract uses failed string deserializer, when it returns answer
        Assert.Multiple(() =>
        {
            Assert.Throws<RemoteSerializationException>(() => tcpPair.OriginContract.OnAskS("testString"));
            tcpPair.AssertPairIsDisconnected();
        });
    }

    [Test]
    public void SerializationThrowsRule_throws()
    {
        //Just check the Moq generator behaves as expected
        var rule = IntegrationTestsHelper.GetThrowsSerializationRuleFor<string>();
        var serializer = rule.GetSerializer(typeof(string), SerializerFactory.CreateDefault());
        Assert.Throws<Exception>(() => serializer.Serialize(new object(), new MemoryStream()));
    }

    [Test]
    public void DeserializationThrowsRule_throws()
    {
        //Just check the Moq generator behaves as expected
        var rule = IntegrationTestsHelper.GetThrowsDeserializationRuleFor<string>();
        var deserializer = rule.GetDeserializer(typeof(string), DeserializerFactory.CreateDefault());
        Assert.Throws<Exception>(() => deserializer.Deserialize(new MemoryStream(), 0));
    }
}