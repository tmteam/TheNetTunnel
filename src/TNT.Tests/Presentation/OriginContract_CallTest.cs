using NUnit.Framework;
using TNT.Presentation.Origin;
using TNT.Tests.Presentation.Origin.OriginContracts;

namespace TNT.Tests.Presentation.Origin
{
    [TestFixture]
    public class OriginContract_CallTest
    {
        private CallContract _contract;
        private CordInterlocutorMock _interlocutor;
        [SetUp]
        public void Initialize()
        {
            _interlocutor = new CordInterlocutorMock();
            _contract = new CallContract();

            OriginContractLinker.Link<ICallContract>(_contract, _interlocutor);
        }
        [Test]
        public void InterlocutorRaisedSayIntStringMethod_ContractMethodCalled()
        {
            int arg1 = 42;
            string arg2 = "42";
            _interlocutor.Raise(CallContract.SayIntStringId, arg1, arg2);
            Assert.IsTrue(_contract.SayIntStringCalled);

            Assert.Multiple(
                () => {
                   Assert.AreEqual(arg1, _contract.SayIntArg);
                   Assert.AreEqual(arg2, _contract.SayStringArg);
                });
        }
        [Test]
        public void InterlocutorRaisedSayVoidMethod_ContractMethodCalled()
        {
            _interlocutor.Raise(CallContract.SayVoidId);
            Assert.IsTrue(_contract.SayVoidCalled);
        }

        [Test]
        public void InterlocutorRaisedAskVoidMethod_ContractMethodReturns()
        {
            var returnedValue = _interlocutor.Raise<double>(CallContract.AskVoidId);
            Assert.AreEqual(CallContract.AskVoidReturns, returnedValue);
        }

        [Test]
        public void InterlocutorRaisedAskSummMethod_ContractMethodReturns()
        {
            double a = 1;
            double b = 2;

            var returnedValue = _interlocutor.Raise<double>(CallContract.AskSummId, a ,b);
            Assert.AreEqual(a+b, returnedValue);
        }
    }
}
