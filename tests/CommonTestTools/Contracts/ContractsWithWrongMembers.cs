using System;
using TNT;

namespace CommonTestTools.Contracts
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
        [TntMessage(1)]
        int propertyWithoutAttribute { get; set; }
    }
    public interface IContractWithSameAskAndEventId
    {
        [TntMessage(1)]
        string Ask();

        [TntMessage(1)]
        Func<int> OnAsk { get; set; }
    }
    public interface IContractWithSameEventId
    {
        [TntMessage(1)]
        Action OnSay { get; set; }

        [TntMessage(1)]
        Func<int> OnAsk { get; set; }
    }
    public interface IContractWithSameSayId
    {
        [TntMessage(1)]
        void Say1();

        [TntMessage(1)]
        void Say2();
    }
    public interface IUnserializeableContract
    {
        [TntMessage(1)]
        void Say(EventArgs arg);
    }

    public interface IUnDeserializeableContract
    {
        [TntMessage(1)]
        EventArgs Ask();
    }
}
