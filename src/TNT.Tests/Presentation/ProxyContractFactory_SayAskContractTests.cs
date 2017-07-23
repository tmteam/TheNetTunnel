using System.Collections;
using System.Linq;
using NUnit.Framework;
using TNT.Contract.Proxy;
using TNT.Tests.Presentation.Contracts;

namespace TNT.Tests.Presentation
{
    [TestFixture]
    public class ProxyContractFactoryWithAnyParametrsTests
    {
        private CordInterlocutorMock _cordMock;
        private ISayAskContract _contract;

        [SetUp]
        public void InitializeProxyContractFactory()
        {
            _cordMock = new CordInterlocutorMock();
            _contract = ProxyContractFactory.CreateProxyContract<ISayAskContract>(_cordMock);
        }

       #region ------------------- Say -------------------

        [Test]
        public void SaySomething1_calledWithCorrectArguments()
        {
            const int    parametrInt    = 100;
            const double parametrDouble = 1032.342;
            const string parametrString = "Hi!!";
            const float  parametrFloat  = (float) 48484.01;
            const bool   parametrBool   = true;

            _contract.SaySomething1(parametrInt, parametrString, parametrDouble, parametrFloat, parametrBool);

            var call = _cordMock.Calls.FirstOrDefault();

            Assert.IsNotNull(call);

            Assert.AreEqual(call.Arguments[0], parametrInt);
            Assert.AreEqual(call.Arguments[1], parametrString);
            Assert.AreEqual(call.Arguments[2], parametrDouble);
            Assert.AreEqual(call.Arguments[3], parametrFloat);
            Assert.AreEqual(call.Arguments[4], parametrBool);
        }

        [Test]
        public void SaySomething1_CalledWithCorrectCordId()
        {
            _contract.SaySomething1(12, "sdsd", 4555.5, 89, false);

            var call = _cordMock.Calls.FirstOrDefault();

            Assert.IsNotNull(call);
            Assert.AreEqual(CordInterlocutorMock.SaySomething1Id, call.CordId);
        }
        
        [Test]
        public void SaySomethingWithArray_ArrayIsAreEqual()
        {
            var objectArray = new object[] { 1045, 1552.544, 56, false};
            var strArray    = new[] {"Fsdfsdf", "Fffsfsdf"};

            _contract.SaySomethingWithArray(objectArray, strArray);

            var call = _cordMock.Calls.FirstOrDefault();

            Assert.IsNotNull(call);

            CollectionAssert.AreEqual((IEnumerable)call.Arguments[0], objectArray);
            CollectionAssert.AreEqual((IEnumerable)call.Arguments[1], strArray);
        }

        #endregion 

        #region ------------------- Ask -------------------

        [Test]
        public void AskSomething_calledWithCorrectArguments()
        {
            const int    parametrInt    = 100;
            const double parametrDouble = 1032.342;
            const string parametrString = "Hi!!";
            const float  parametrFloat  = (float)48484.01;
            const bool   parametrBool   = true;

            _contract.AskSomething1(parametrInt, parametrString, parametrDouble, parametrFloat, parametrBool);

            var call = _cordMock.Calls.FirstOrDefault();

            Assert.IsNotNull(call);

            Assert.AreEqual(call.Arguments[0], parametrInt);
            Assert.AreEqual(call.Arguments[1], parametrString);
            Assert.AreEqual(call.Arguments[2], parametrDouble);
            Assert.AreEqual(call.Arguments[3], parametrFloat);
            Assert.AreEqual(call.Arguments[4], parametrBool);
        }


        [Test]
        public void AskSomething_returnsCorrectValue()
        {
            const string expectedAns = "fortytwo";

            _cordMock.SetAskAnswer(CordInterlocutorMock.AskMessage1Id, expectedAns);

            var res = _contract.AskSomething1(100, "200", 300.565, 400, true);
            Assert.AreEqual(expectedAns, res);
        }

        [Test]
        public void AskSomething1_CalledWithCorrectCordId()
        {
            _contract.AskSomething1(12, "sdsd", 4555.5, 89, false);

            var call = _cordMock.Calls.FirstOrDefault();

            Assert.IsNotNull(call);
            Assert.AreEqual(CordInterlocutorMock.AskMessage1Id, call.CordId);
        }

        [Test]
        public void AskSomethingWithArray_ArrayIsAreEqual()
        {
            var objectArray = new object[] { 1045, 1552.544, 56, false };
            var strArray    = new[] { "Fsdfsdf", "Fffsfsdf" };

            _contract.AskSomethingWithArray(objectArray, strArray);
            
            var call = _cordMock.Calls.FirstOrDefault();

            Assert.IsNotNull(call);

            CollectionAssert.AreEqual((IEnumerable)call.Arguments[0], objectArray);
            CollectionAssert.AreEqual((IEnumerable)call.Arguments[1], strArray);
        }

        #endregion 
    }
}