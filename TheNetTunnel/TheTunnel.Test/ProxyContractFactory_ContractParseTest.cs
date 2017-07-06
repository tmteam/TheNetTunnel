using EmitExperiments;
using EmitExperiments.Exceptions;
using NUnit.Framework;
using TheTunnel.Test.ContractInterfaces;

namespace TheTunnel.Test
{
    [TestFixture]
    public class ProxyContractFactory_ContractParseTest
    {

        [Test]
        public void EmptyContract_Creates()
        {
            var stub = new CordMock();
            var proxy = ProxyContractFactory.CreateProxyContract<IEmptyContract>(stub);
            Assert.IsNotNull(proxy);
        }
        [Test]
        public void SayCordIdDuplicated_CreateT_throwsException()
        {
            var stub = new CordMock();
            Assert.Throws<ContractCordIdDuplicateException>(
                ()=> ProxyContractFactory.CreateProxyContract<IContractWithSameSayId>(stub));
        }

        [Test]
        public void EventCordIdDuplicated_CreateT_throwsException()
        {
            var stub = new CordMock();
            Assert.Throws<ContractCordIdDuplicateException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithSameEventId>(stub));
        }

        [Test]
        public void AskAndEventCordIdDuplicated_CreateT_throwsException()
        {
            var stub = new CordMock();
            Assert.Throws<ContractCordIdDuplicateException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithSameAskAndEventId>(stub));
        }

        [Test]
        public void PropertyDoesNotContainAttribute_CreateT_throwsException()
        {
            var stub = new CordMock();
            Assert.Throws<ContractMemberAttributeMissingException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithPropertyWithoutAttribute>(stub));
        }
        [Test]
        public void EventDoesNotContainAttribute_CreateT_throwsException()
        {
            var stub = new CordMock();
            Assert.Throws<ContractMemberAttributeMissingException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithEventWithoutAttribute>(stub));
        }
        [Test]
        public void MethodDoesNotContainAttribute_CreateT_throwsException()
        {
            var stub = new CordMock();
            Assert.Throws<ContractMemberAttributeMissingException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithMethodWithoutAttribute>(stub));
        }
        [Test]
        public void DelegateDoesNotContainAttribute_CreateT_throwsException()
        {
            var stub = new CordMock();
            Assert.Throws<ContractMemberAttributeMissingException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithDelegateWithoutAttribute>(stub));
        }
        [Test]
        public void ContractWithNonDelegateProperty_CreateT_throwsException()
        {
            var stub = new CordMock();
            Assert.Throws<InvalidContractMemeberException>(
                () => ProxyContractFactory.CreateProxyContract<IContractWithNonDelegateProperty>(stub));
        }
    }
}
