using CommonTestTools;
using CommonTestTools.Contracts;
using NUnit.Framework;
using TNT.Contract.Proxy;
using TNT.Exceptions.ContractImplementation;

namespace TNT.Core.Tests.Presentation
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
            Assert.Throws<ContractMessageIdDuplicateException>(
                ()=> ProxyContractFactory.CreateProxyContract<IContractWithSameSayId>(stub));
        }

        [Test]
        public void EventCordIdDuplicated_CreateT_throwsException()
        {
            var stub = new CordInterlocutorMock();
            Assert.Throws<ContractMessageIdDuplicateException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithSameEventId>(stub));
        }

        [Test]
        public void AskAndEventCordIdDuplicated_CreateT_throwsException()
        {
            var stub = new CordInterlocutorMock();
            Assert.Throws<ContractMessageIdDuplicateException>(
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
