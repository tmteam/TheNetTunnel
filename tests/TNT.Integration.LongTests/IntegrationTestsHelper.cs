using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CommonTestTools.Contracts;
using Moq;
using Tnt.LongTests.ContractMocks;
using TNT;
using TNT.Api;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace Tnt.LongTests
{
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
            var throwsRule = new SerializationRule(t => t == typeof(T), t => fakeSerializer.Object);
            return throwsRule;
        }
        public static DeserializationRule GetThrowsDeserializationRuleFor<T>()
        {
            var fakeDeserializer = new Mock<IDeserializer>();

            fakeDeserializer
                .Setup(s => s.Deserialize(It.IsAny<Stream>(), It.IsAny<int>()))
                .Callback(() => { throw new Exception(); });
            var throwsRule = new DeserializationRule(t => t == typeof(T), t => fakeDeserializer.Object);
            return throwsRule;
        }
        public static Thread[] RunInParrallel<Targ>(int concurrentCount, Func<int, Targ> initializeAction, Action<int, Targ> action)
        {
            var start = new ManualResetEvent(false);
            Exception inThreadsException = null;
            List<Thread> threads = new List<Thread>(concurrentCount);
            for (int i = 0; i < concurrentCount; i++)
            {
                var thread = new Thread(_ =>
                {
                    try
                    {
                        var arg = initializeAction(i);
                        start.WaitOne();
                        action(i, arg);
                    }
                    catch (Exception e)
                    {
                        inThreadsException = e;
                    }
                })
                {
                    Name = "Concurrent#" + i
                };
                thread.Start();
                threads.Add(thread);
            }
            if (inThreadsException != null)
                throw inThreadsException;
            start.Set();
            if (inThreadsException != null)
                throw inThreadsException;
            return threads.ToArray();
        }
        public static void CreateTwoConnectedTcpClients(int port, out TcpClient clientA, out TcpClient clientB)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            var listenTask = listener.AcceptTcpClientAsync();
            clientA = new TcpClient();
            var connectTask = clientA.ConnectAsync(IPAddress.Loopback, port);

            if (!listenTask.Wait(1000))
            {
                throw new InvalidOperationException();
            }

            clientB = listenTask.Result;
            listener.Stop();
        }
        public static void WaitOrThrow(Func<bool> condition, Func<Exception> exceptionLocator)
        {
            while (!condition())
            {
                Thread.Sleep(1);
                var ex = exceptionLocator();
                if (ex != null)
                    throw ex;
            }
        }
        public static byte[] CreateArray(int length, byte value)
        {
            var array = new byte[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = value;
            }

            return array;
        }
    }
}
