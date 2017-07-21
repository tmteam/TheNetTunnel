using NUnit.Framework;
using TNT.Contract.Proxy;
using TNT.Exceptions;
using TNT.Exceptions.ContractImplementation;
using TNT.Tests.Presentation.Proxy.ContractInterfaces;

namespace TNT.Tests.Presentation.Proxy
{
    [TestFixture]
    public class ProxyContractFactory_ContractParseTest
    {

        [Test]
        public void EmptyContract_Creates()
        {
            var stub = new CordInterlocutorMock();
            var proxy = ProxyContractFactory.CreateProxyContract<IEmptyContract>(stub);
            Assert.IsNotNull(proxy);
        }
        [Test]
        public void SayCordIdDuplicated_CreateT_throwsException()
        {
            var stub = new CordInterlocutorMock();
            Assert.Throws<ContractCordIdDuplicateException>(
                ()=> ProxyContractFactory.CreateProxyContract<IContractWithSameSayId>(stub));
        }

        [Test]
        public void EventCordIdDuplicated_CreateT_throwsException()
        {
            var stub = new CordInterlocutorMock();
            Assert.Throws<ContractCordIdDuplicateException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithSameEventId>(stub));
        }

        [Test]
        public void AskAndEventCordIdDuplicated_CreateT_throwsException()
        {
            var stub = new CordInterlocutorMock();
            Assert.Throws<ContractCordIdDuplicateException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithSameAskAndEventId>(stub));
        }

        [Test]
        public void PropertyDoesNotContainAttribute_CreateT_throwsException()
        {
            var stub = new CordInterlocutorMock();
            Assert.Throws<ContractMemberAttributeMissingException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithPropertyWithoutAttribute>(stub));
        }
       
        [Test]
        public void MethodDoesNotContainAttribute_CreateT_throwsException()
        {
            var stub = new CordInterlocutorMock();
            Assert.Throws<ContractMemberAttributeMissingException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithMethodWithoutAttribute>(stub));
        }
        [Test]
        public void DelegateDoesNotContainAttribute_CreateT_throwsException()
        {
            var stub = new CordInterlocutorMock();
            Assert.Throws<ContractMemberAttributeMissingException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithDelegateWithoutAttribute>(stub));
        }
        [Test]
        public void ContractWithNonDelegateProperty_CreateT_throwsException()
        {
            var stub = new CordInterlocutorMock();
            Assert.Throws<InvalidContractMemeberException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithNonDelegateProperty>(stub));
        }
    
    }
}
