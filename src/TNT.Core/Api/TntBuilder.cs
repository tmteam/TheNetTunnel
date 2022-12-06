using System;
using TNT.Api;
using TNT.Transport;

// ReSharper disable once CheckNamespace
namespace TNT;

public static class TntBuilder
{
    public static PresentationBuilder<TContract> UseContract<TContract>()
        where TContract : class
    {
        return new PresentationBuilder<TContract>();
    }

    public static PresentationBuilder<TContract> UseContract<TContract, TImplementation>() 
        where TContract: class 
        where TImplementation: TContract, new()
    {
        return UseContract<TContract>((c) => new TImplementation());
    }
    
    public static PresentationBuilder<TContract> UseContract<TContract>(TContract implementation)
        where TContract : class
    {
        return UseContract((c) => implementation);
    }
    
    public static PresentationBuilder<TContract> UseContract<TContract>(Func<TContract> implementationFactory)
        where TContract : class
    {
        return UseContract((c) => implementationFactory());
    }

    public static PresentationBuilder<TContract> UseContract<TContract>(Func<IChannel, TContract> implementationFactory)
        where TContract : class
    {
        return new PresentationBuilder<TContract>(implementationFactory);
    }
}