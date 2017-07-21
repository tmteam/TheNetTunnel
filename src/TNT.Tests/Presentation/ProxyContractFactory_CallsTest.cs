using NUnit.Framework;
using TNT.Contract.Proxy;
using TNT.Tests.Presentation.Proxy.ContractInterfaces;

namespace TNT.Tests.Presentation.Proxy
{
    [TestFixture]
    public class ProxyContractFactory_CallsTest
    {
        private CordInterlocutorMock      _cordMock;
        private ICallContract2 _contract;

        [SetUp]
        public void InitializeProxyContractFactory()
        {
            _cordMock = new CordInterlocutorMock();
            _contract = ProxyContractFactory.CreateProxyContract<ICallContract2>(_cordMock);
        }

        [Test]
        public void ProcedureCallNotSubscribed_MockRaises_notThrows()
        {
            _cordMock.Raise(CordInterlocutorMock.ProcedureCallId);
        }
        [Test]
        public void ArgumentProcedureCallNotSubscribed_MockRaises_notThrows()
        {
            _cordMock.Raise(CordInterlocutorMock.ArgumentProcedureCallId,42,"42");
        }

        [Test]
        public void FuncCallReturnsIntNotSubscribed_MockRaises_ReturnsDefault()
        {
            var res = _cordMock.Raise<int>(CordInterlocutorMock.FuncCallReturnsIntId);
            Assert.AreEqual(0,res);
        }
        [Test]
        public void FuncCallReturnsBoolNotSubscribed_MockRaises_ReturnsDefault()
        {
            var res = _cordMock.Raise<bool>(CordInterlocutorMock.FuncCallReturnsBoolId);
            Assert.AreEqual(false, res);
        }
        [Test]
        public void FuncCallRefTypeNotSubscribed_MockRaises_ReturnsDefault()
        {
            var res = _cordMock.Raise<object>(CordInterlocutorMock.FuncCallReturnsRefTypeId);
            Assert.IsNull(res);
        }

        [Test]
        public void FuncReturnsArrayCallNotSubscribed_MockRaises_ReturnsDefault()
        {
            var res = _cordMock.Raise<int[]>(CordInterlocutorMock.FuncReturnsArrayCallId);
            Assert.IsNull(res);
        }


        [Test]
        public void MockRaises_procedureCalledOneTime()
        {
            int raised = 0;
            _contract.ProcedureCall += () => raised++;
            _cordMock.Raise(CordInterlocutorMock.ProcedureCallId);
            Assert.AreEqual(1,raised);
        }
        [Test]
        public void MockRaisesTwice_procedureCalledTwice()
        {
            int raised = 0;
            _contract.ProcedureCall += () => raised++;
            _cordMock.Raise(CordInterlocutorMock.ProcedureCallId);
            _cordMock.Raise(CordInterlocutorMock.ProcedureCallId);
            Assert.AreEqual(2, raised);
        }

        [Test]
        public void MockRaises_ArgumentProcedureCall_CalledWithCorrectArguments()
        {
            int? intReceivedArg = null;
            string stringReceivedArg = null;
            _contract.ArgumentProcedureCall += (a,b) =>
            {
                intReceivedArg = a;
                stringReceivedArg =b;
            };

            int intSendArg = 42;
            string stringSendArg = "fortyTwo";

            _cordMock.Raise(CordInterlocutorMock.ArgumentProcedureCallId, intSendArg, stringSendArg);
            Assert.AreEqual(intSendArg, intReceivedArg);
            Assert.AreEqual(stringSendArg, stringReceivedArg);
        }
        [Test]
        public void MockRaises_ArrayArgumentProcedureCall_CalledWithCorrectArguments()
        {
            int[] intReceivedArg = null;
            string[] stringReceivedArg = null;
            _contract.ArrayArgumentProcedureCall += (a, b) =>
            {
                intReceivedArg = a;
                stringReceivedArg = b;
            };

            var intSendArg = new[]{ 42,43,44};
            var stringSendArg = new[] {"fortyTwo","fortyThree","fortyFour"};

            _cordMock.Raise(CordInterlocutorMock.ArrayArgumentProcedureCallId, intSendArg, stringSendArg);
            CollectionAssert.AreEqual(intSendArg, intReceivedArg);
            CollectionAssert.AreEqual(stringSendArg, stringReceivedArg);
        }

        [Test]
        public void MockRaises_FuncCallReturnsValueType_returnsCorrectValue()
        {
            int returnValue = 42;
            _contract.FuncCallReturnsInt += () => returnValue;

            var res = _cordMock.Raise<int>(CordInterlocutorMock.FuncCallReturnsIntId);
            Assert.AreEqual(returnValue , res);
        }

        [Test]
        public void MockRaises_FuncCallReturnsRefType_returnsCorrectValue()
        {
            string returnValue = "42";
            _contract.FuncCallReturnsRefType += () => returnValue;

            var res = _cordMock.Raise<string>(CordInterlocutorMock.FuncCallReturnsRefTypeId);
            Assert.AreEqual(returnValue, res);
        }

        [Test]
        public void MockRaises_ArgumentFuncCall_receivedCorrectArguments()
        {
            int? receivedValue1 = null;
            string receivedValue2 = null;
            _contract.ArgumentFuncCall += (a,b) =>
            {
                receivedValue1 = a;
                receivedValue2 = b;
                return 0;
            };
            int sendValue1 = 42;
            string sendValue2 = "42";
           _cordMock.Raise<int>(CordInterlocutorMock.ArgumentFuncCallId, sendValue1,sendValue2);
            Assert.AreEqual(sendValue1,receivedValue1);
            Assert.AreEqual(sendValue2, receivedValue2);
        }

        [Test]
        public void MockRaises_FuncReturnsArrayCall_returnsCorrectValue()
        {
            var returnValue = new[]{ 42,43,44};
            _contract.FuncReturnsArrayCall += () => returnValue;

            var res = _cordMock.Raise<int[]>(CordInterlocutorMock.FuncReturnsArrayCallId);
            CollectionAssert.AreEqual(returnValue, res);
        }
    }
}