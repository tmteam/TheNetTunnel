using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TNT.Api;
using TNT.Presentation;
using TNT.Tcp;
using TNT.Tests;

namespace TNT.IntegrationTests.Serialization
{
    [TestFixture()]
    public class BigProtobuffConcurentCallTest
    {
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(500)]
        [TestCase(800)]
        [TestCase(900)]
        [TestCase(990)]
        [TestCase(1024)]
        [TestCase(2*1024)]
        [TestCase(ushort.MaxValue*2)] 
        public void StringPackets_transmitsViaTcp_concurent(int stringLengthInBytes)
        {
            using (var tcpPair = new TcpConnectionPair
                <ISingleMessageContract<string>,
                ISingleMessageContract<string>,
                SingleMessageContract<string>>())
            {
                //Tasks count:
                int sentCount = 40;
                
                string originStringArgument = generateRandomString(stringLengthInBytes / 2);

                var receivedList = new List<string>(200);
                tcpPair.OriginContract.SayCalled
                   += (Sender, received) => receivedList.Add(received);

                //start monitoring disconnection events. 
                var serverDisconnectAwaiter
                    = new EventAwaiter<ClientDisconnectEventArgs<ISingleMessageContract<string>, TcpChannel>>();
                tcpPair.Server.Disconnected += serverDisconnectAwaiter.EventRaised;

                var clientDisconnectAwaiter
                   = new EventAwaiter<ErrorMessage>();
                tcpPair.ClientChannel.OnDisconnect += clientDisconnectAwaiter.EventRaised;

                #region sending
                var sentTasks = new List<Task>(sentCount);
                for (var i = 0; i < sentCount; i++)
                    sentTasks.Add(new Task(() => tcpPair.ProxyConnection.Contract.Ask(originStringArgument)));
                foreach (var task in sentTasks)
                    task.Start();
                #endregion


                #region checking for exceptions

                bool allDoneSucc = false;
                Exception doneException = null;
                try
                {
                    allDoneSucc = Task.WaitAll(sentTasks.ToArray(), 60000);
                }
                catch (Exception e) { doneException = e; }
                //Check for client disconnection
                var clientDisconnectArgs = clientDisconnectAwaiter.WaitOneOrDefault(500);

                //Check for server disconnection
                var serverDisconnectedArg = serverDisconnectAwaiter.WaitOneOrDefault(500);
                if (clientDisconnectArgs != null || serverDisconnectedArg != null)
                {
                    if (clientDisconnectArgs != null)
                        Assert.Fail("Client disconnected. Reason: " + clientDisconnectArgs);
                    else if (serverDisconnectedArg != null)
                        Assert.Fail("Server disconnected. Reason: " + serverDisconnectedArg.ErrorMessageOrNull);
                }
                //check for tasks agregate exception
                if (doneException != null)
                    Assert.Fail("Client side thrown exception during the interaction: " + doneException.ToString());
                //Check for timeout
                if (!allDoneSucc)
                    Assert.Fail("Test timeout ");

                #endregion
                
                #region checking for  serialization results
                Assert.AreEqual(sentCount, receivedList.Count);
                foreach (var received in receivedList)
                {
                   Assert.AreEqual(originStringArgument, received);
                }
                #endregion
            }
        }
        [TestCase(1, 40)]
        [TestCase(10, 40)]
        [TestCase(20, 40)]
        [TestCase(50, 40)]
        [TestCase(80, 40)]
        [TestCase(100, 40)]
        [TestCase(200, 40)]
        [TestCase(400, 40)]
        [TestCase(1000, 40)]
        public void ProtobuffPackets_transmitViaTcp_concurent(int sizeOfCompanyInUsers, int parralelTasksCount)
        {
            //1000 = 0.5mb  
            //2000 = 2mb    
            //5000 = 10mb   
            //10000 = 50mb  5

            var origin = TntBuilder
                .UseContract<ISingleMessageContract<Company>, SingleMessageContract<Company>>()
                .SetMaxAnsDelay(5 * 60 * 1000);
            var proxy = TntBuilder
                .UseContract<ISingleMessageContract<Company>>()
                .SetMaxAnsDelay(5 * 60 * 1000);

            using (var tcpPair = new TcpConnectionPair
                <ISingleMessageContract<Company>,
                ISingleMessageContract<Company>,
                SingleMessageContract<Company>>(origin,proxy))
            {
                var originCompany = IntegrationTestsHelper.CreateCompany(sizeOfCompanyInUsers);
                var receivedList = new List<Company>(parralelTasksCount);
                tcpPair.OriginContract.SayCalled
                   += (Sender, received) => receivedList.Add(received);

                //start monitoring the server disconnection event. 
                var serverDisconnectAwaiter 
                    = new EventAwaiter<ClientDisconnectEventArgs<ISingleMessageContract<Company>, TcpChannel>>();
                tcpPair.Server.Disconnected += serverDisconnectAwaiter.EventRaised;

                var clientDisconnectAwaiter
                   = new EventAwaiter<ErrorMessage>();
                tcpPair.ClientChannel.OnDisconnect += clientDisconnectAwaiter.EventRaised;

                #region sending
                var sentTasks = new List<Task>(parralelTasksCount);
                for (var i = 0; i < parralelTasksCount; i++)
                    sentTasks.Add(new Task(() => tcpPair.ProxyConnection.Contract.Ask(originCompany)));
                foreach (var task in sentTasks)
                    task.Start();
                #endregion

                #region checking for exceptions

                bool allDoneSucc = false;
                Exception doneException = null;
                try
                {
                    allDoneSucc = Task.WaitAll(sentTasks.ToArray(), 60000);
                }
                catch (Exception e) { doneException = e; }
                //Check for client disconnection
                var clientDisconnectArgs = clientDisconnectAwaiter.WaitOneOrDefault(500);
                
                //Check for server disconnection
                var serverDisconnectedArg = serverDisconnectAwaiter.WaitOneOrDefault(500);
                if (clientDisconnectArgs != null || serverDisconnectedArg != null)
                {
                    if(clientDisconnectArgs!=null)
                        Assert.Fail("Client disconnected. Reason: " + clientDisconnectArgs);
                    else if(serverDisconnectedArg!=null)
                        Assert.Fail("Server disconnected. Reason: " + serverDisconnectedArg.ErrorMessageOrNull);
                }
                //check for tasks agregate exception
                if (doneException != null)
                    Assert.Fail("Client side thrown exception during the interaction: " + doneException.ToString());
                //Check for timeout
                if (!allDoneSucc)
                    Assert.Fail("Test timeout ");

                #endregion


                #region checking for  serialization results
                Assert.AreEqual(parralelTasksCount, receivedList.Count);
                foreach (var received in receivedList) {
                    received.AssertIsSameTo(originCompany);
                }
                #endregion
            }
        }
        [Test]
        public void HundredOf2mbPacket_transmitsViaTcp_oneByOne()
        {
            var origin = TntBuilder
               .UseContract<ISingleMessageContract<Company>, SingleMessageContract<Company>>()
               .SetMaxAnsDelay(5 * 60 * 1000);
            var proxy = TntBuilder
                .UseContract<ISingleMessageContract<Company>>()
                .SetMaxAnsDelay(5 * 60 * 1000);
            using (var tcpPair = new TcpConnectionPair
                <ISingleMessageContract<Company>,
                ISingleMessageContract<Company>,
                SingleMessageContract<Company>>(origin,proxy))
            {
                var receivedList = new List<Company>(200);

                tcpPair.OriginContract.SayCalled 
                    += (Sender, received) => receivedList.Add(received);
                var company =  IntegrationTestsHelper.CreateCompany(2000);
                int sendCount = 100;
                for(int i = 0; i< sendCount; i++)
                    tcpPair.ProxyConnection.Contract.Ask(company);
                Assert.AreEqual(sendCount, receivedList.Count);
                foreach (var received in receivedList)
                {
                    received.AssertIsSameTo(company);
                }
            }
        }
       
        [Test]
        public void HundredOf2mbPacket_transmitsViaTcp_concurent()
        {
            var origin = TntBuilder
               .UseContract<ISingleMessageContract<Company>, SingleMessageContract<Company>>()
               .SetMaxAnsDelay(5 * 60 * 1000);
            var proxy = TntBuilder
                .UseContract<ISingleMessageContract<Company>>()
                .SetMaxAnsDelay(5 * 60 * 1000);
            using (var tcpPair = new TcpConnectionPair
                <ISingleMessageContract<Company>,
                ISingleMessageContract<Company>,
                SingleMessageContract<Company>>(origin, proxy))
            {
                var receivedList = new List<Company>(200);

                tcpPair.OriginContract.SayCalled
                    += (Sender, received) => receivedList.Add(received);
                var company = IntegrationTestsHelper.CreateCompany(2000);
                int sendCount = 100;

                List<Task> sendTasks = new List<Task>(sendCount);
                for (int i = 0; i < sendCount; i++)
                    sendTasks.Add(new Task(()=>tcpPair.ProxyConnection.Contract.Ask(company)));

                foreach (Task task in sendTasks)
                {
                    task.Start();
                }
                if (!Task.WaitAll(sendTasks.ToArray(), 5*60*1000))
                {
                    Assert.Fail("Test timeout ");
                }


                Assert.AreEqual(sendCount, receivedList.Count);
                foreach (var received in receivedList)
                {
                    received.AssertIsSameTo(company);
                }
            }
        }


        public String generateRandomString(int length)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            //Initiate objects & vars    Random random = new Random();
            String randomString = "";
            int randNumber;

            //Loop ‘length’ times to generate a random number or character
            for (int i = 0; i < length; i++)
            {
                if (random.Next(1, 3) == 1)
                    randNumber = random.Next(97, 123); //char {a-z}
                else
                    randNumber = random.Next(48, 58); //int {0-9}

                //append random char or digit to random string
                randomString = randomString + (char)randNumber;
            }
            //return the random string
            return randomString;
        }
    }
}
