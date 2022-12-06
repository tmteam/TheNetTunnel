using System;
using CommonTestTools;
using CommonTestTools.Contracts;
using NUnit.Framework;
using TNT;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.ReceiveDispatching;
using TNT.Tcp;

namespace Tnt.LongTests.TcpLocalhostSpecificTest;

[TestFixture]
public class CallsExceptionsTest
{
    [Test]
    public void ProxyConnectionIsNotEstablishedYet_SayCallThrows()
    {
        var proxyConnection = TntBuilder
            .UseContract<ITestContract>()
            .UseReceiveDispatcher<NotThreadDispatcher>()
            .UseChannel(() => new TcpChannel())
            .Build();

        TestTools.AssertThrowsAndNotBlocks<ConnectionIsNotEstablishedYet>(() => proxyConnection.Contract.Say());
    }

    [Test]
    public void ProxyConnectionIsNotEstablishedYet_AskCallThrows()
    {
        var proxyConnection = TntBuilder
            .UseContract<ITestContract>()
            .UseReceiveDispatcher<NotThreadDispatcher>()
            .UseChannel(() => new TcpChannel())
            .Build();

        TestTools.AssertThrowsAndNotBlocks<ConnectionIsNotEstablishedYet>(() => proxyConnection.Contract.Ask());
    }

    [Test]
    public void ProxyConnectionIsLost_SayCallThrows()
    {
        using var tcpPair = new TcpConnectionPair();
        tcpPair.Disconnect();
        TestTools.AssertThrowsAndNotBlocks<ConnectionIsLostException>(
            () => tcpPair.ProxyConnection.Contract.Say());
    }

    [Test]
    public void ProxyConnectionIsLost_AskCallThrows()
    {
        using var tcpPair = new TcpConnectionPair();
        tcpPair.Disconnect();
        TestTools.AssertThrowsAndNotBlocks<ConnectionIsLostException>(
            () => tcpPair.ProxyConnection.Contract.Ask());
    }

    [Test]
    public void Proxy_AskMissingCord_Throws()
    {
        using var tcpPair = new TcpConnectionPair<ITestContract, IEmptyContract, EmptyContract>();
        Assert.Multiple(() =>
        {
            TestTools.AssertThrowsAndNotBlocks<RemoteContractImplementationException>(
                () => tcpPair.ProxyConnection.Contract.Ask());
            tcpPair.AssertPairIsConnected();
        });
    }
    [Test]
    public void Proxy_SayMissingCord_NotThrows()
    {
        using var tcpPair = new TcpConnectionPair<ITestContract, IEmptyContract, EmptyContract>();
        Assert.Multiple(() =>
        {
            TestTools.AssertNotBlocks(
                () => tcpPair.ProxyConnection.Contract.Say());
            tcpPair.AssertPairIsConnected();
        });
    }
    
    [Test]
    public void Origin_SayMissingCord_NotThrows()
    {
        using var tcpPair = new TcpConnectionPair<IEmptyContract, ITestContract, TestContractMock>();
        Assert.Multiple(() =>
        {
            TestTools.AssertNotBlocks(
                () => tcpPair.OriginContract.OnSay());
            tcpPair.AssertPairIsConnected();
        });
    }
    
    [Test]
    public void Proxy_SayWithException_CallNotThrows()
    {
        using var tcpPair = new TcpConnectionPair<IExceptionalContract, IExceptionalContract, ExceptionalContract>();
        Assert.Multiple(() =>
        {
            TestTools.AssertNotBlocks(
                () => tcpPair.ProxyConnection.Contract.Say());
            tcpPair.AssertPairIsConnected();
        });
    }
    
    [Test]
    public void Proxy_AskWithException_Throws()
    {
        using var tcpPair = new TcpConnectionPair<IExceptionalContract, IExceptionalContract, ExceptionalContract>();
        Assert.Multiple(() =>
        {
            TestTools.AssertThrowsAndNotBlocks<RemoteUnhandledException>(
                () => tcpPair.ProxyConnection.Contract.Ask());
            tcpPair.AssertPairIsConnected();
        });
    }

    [Test]
    public void Origin_SayExceptioanlCallback_NotThrows()
    {
        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>();
        tcpPair.ProxyContract.OnSay += () => { throw new InvalidOperationException(); };
        Assert.Multiple(() =>
        {
            TestTools.AssertNotBlocks(
                () => tcpPair.OriginContract.OnSay());
            tcpPair.AssertPairIsConnected();
        });
    }

    [Test]
    public void Origin_AskExceptioanlCallback_Throws()
    {
        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>();
        Assert.Multiple(() =>
        {
            tcpPair.ProxyContract.OnAsk += () => { throw new InvalidOperationException(); };
            TestTools.AssertThrowsAndNotBlocks<RemoteUnhandledException>(
                () => tcpPair.OriginContract.OnAsk());
            tcpPair.AssertPairIsConnected();
        });
    }
    [Test]
    public void Origin_AsksNotImplemented_returnsDefault()
    {
        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>();
        Assert.Multiple(() =>
        {
            var answer = TestTools.AssertNotBlocks(tcpPair.OriginContract.OnAsk).Result;
            Assert.AreEqual(default(int), answer);
            tcpPair.AssertPairIsConnected();
        });
    }

    [Test]
    public void Disconnected_duringOriginAsk_throws()
    {
        using var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>();
        tcpPair.ProxyConnection.Contract.OnAsk += () =>
        {
            tcpPair.Disconnect();
            return 0;
        };

        TestTools.AssertThrowsAndNotBlocks<ConnectionIsLostException>(
            () => tcpPair.OriginContract.OnAsk());
    }
}