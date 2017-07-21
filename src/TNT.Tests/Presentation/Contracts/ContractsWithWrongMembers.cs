using System;
using TNT.Contract;
using TNT.Presentation;

namespace TNT.Tests.Presentation.Proxy.ContractInterfaces
{
    public interface IContractWithMethodWithoutAttribute
    {
        void MethodWithoutAttribute();
    }

    public interface IContractWithDelegateWithoutAttribute
    {
        Action ActionWithoutAttribute { get; set; }
    }
    
    public interface IContractWithEventWithoutAttribute
    {
        event Action ActionWithoutAttribute;
    }

    public interface IContractWithPropertyWithoutAttribute
    {
        int propertyWithoutAttribute { get; set; }
    }

    public interface IContractWithNonDelegateProperty
    {
        [ContractMessage(1)]
        int propertyWithoutAttribute { get; set; }
    }
    public interface IContractWithSameAskAndEventId
    {
        [ContractMessage(1)]
        string Ask();

        [ContractMessage(1)]
        Func<int> OnAsk { get; set; }
    }
    public interface IContractWithSameEventId
    {
        [ContractMessage(1)]
        Action OnSay { get; set; }

        [ContractMessage(1)]
        Func<int> OnAsk { get; set; }
    }
    public interface IContractWithSameSayId
    {
        [ContractMessage(1)]
        void Say1();

        [ContractMessage(1)]
        void Say2();
    }
    public interface IUnserializeableContract
    {
        [ContractMessage(1)]
        void Say(EventArgs arg);
    }

    public interface IUnDeserializeableContract
    {
        [ContractMessage(1)]
        EventArgs Ask();
    }
}
