using System.Linq;
using NUnit.Framework;
using TNT.Contract.Origin;
using TNT.Tests.Presentation.Contracts;

namespace TNT.Tests.Presentation
{
    [TestFixture]
    public class OriginContract_CallbacksTest
    {
        private CallBackContract _contract;
        private CordInterlocutorMock _interlocutor;
        [SetUp]
        public void Initialize()
        {
            _interlocutor = new CordInterlocutorMock();
            _contract = new CallBackContract();

            OriginContractLinker.Link<ICallBackContract>(_contract, _interlocutor);
        }
        [Test]
        public void SayIntStringCallbackCalled_InterlocutorSayMethodCalled()
        {
            int arg1 = 42;
            string arg2 = "42";
            _contract.SayIntString(arg1, arg2);
            var call = _interlocutor.Calls.SingleOrDefault();
            Assert.Multiple(
                () => {
                    Assert.IsNotNull(call);
                    Assert.AreEqual(call.CordId, CallBackContract.SayIntStringCallBackId);
                    Assert.IsInstanceOf<int>(call.Arguments[0]);
                    Assert.IsInstanceOf<string>(call.Arguments[1]);
                    Assert.AreEqual(arg1, (int) call.Arguments[0]);
                    Assert.AreEqual(arg2, (string) call.Arguments[1]);
                });
        }

        [Test]
        public void SayVoidCallbackCalled_InterlocutorSayMethodCalled()
        {
            _contract.SayVoid();
            var call = _interlocutor.Calls.SingleOrDefault();
            Assert.Multiple(
                () => {
                    Assert.IsNotNull(call);
                    Assert.AreEqual(call.CordId, CallBackContract.SayVoidCallBackId);
                    Assert.IsEmpty(call.Arguments);
                });
        }
        [Test]
        public void AskVoidCallbackCalled_InterlocutorReturns42()
        {
            double expectedResult = 42;
            _interlocutor.AskAnwers.Add(CallBackContract.AskVoidId, expectedResult);
            var result = _contract.AskVoid();
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void AskVoidCallbackCalled_InterlocutorAskMethodCalled()
        {
            _interlocutor.AskAnwers.Add(CallBackContract.AskVoidId, (double)42);

            _contract.AskVoid();

            var call = _interlocutor.Calls.SingleOrDefault();
            Assert.Multiple(
                () => {
                    Assert.IsNotNull(call);
                    Assert.AreEqual(call.CordId, CallBackContract.AskVoidId);
                    Assert.IsEmpty(call.Arguments);
                });
        }

        [Test]
        public void AskSummCallbackCalled_InterlocutorReturns()
        {
            double expectedResult = 41;
            _interlocutor.AskAnwers.Add(CallBackContract.AskSummId, expectedResult);
            var result = _contract.AskSumm(007,007);
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void AskSummCallbackCalled_InterlocutorAskMethodCalled()
        {
            _interlocutor.AskAnwers.Add(CallBackContract.AskSummId, (double)31.0);
            double arg1 = 007;
            double arg2 = 777;
            _contract.AskSumm(arg1,arg2);

            var call = _interlocutor.Calls.SingleOrDefault();
            Assert.Multiple(
                () => {
                    Assert.IsNotNull(call);
                    Assert.AreEqual(call.CordId, CallBackContract.AskSummId);
                    Assert.IsInstanceOf<double>(call.Arguments[0]);
                    Assert.IsInstanceOf<double>(call.Arguments[1]);
                    Assert.AreEqual(arg1, (double)call.Arguments[0]);
                    Assert.AreEqual(arg2, (double)call.Arguments[1]);
                });
        }
    }
}
