using System;

namespace TNT.Cord.Deserializers
{
    public class DeserializationRule
    {
        private Predicate<Type> _rule;
        private readonly Func<Type, DeserializerFactory, IDeserializer> _deserializerLocator;


        public DeserializationRule(Predicate<Type> rule, Func<Type, IDeserializer> deserializerLocator)
        {
            _rule = rule;
            _deserializerLocator = (t,f)=>deserializerLocator(t);
        }
        public DeserializationRule(Predicate<Type> rule, Func<Type, DeserializerFactory, IDeserializer> deserializerLocator)
        {
            _rule = rule;
            _deserializerLocator = deserializerLocator;
        }

        public static DeserializationRule Create<T>(IDeserializer<T> deserializer)
        {
            return new DeserializationRule((t)=> t == typeof(T),(t)=> deserializer);
        }

        public Predicate<Type> Rule
        {
            get { return _rule; }
        }

        public IDeserializer GetDeserializer(Type t, DeserializerFactory factory)
        {
            return _deserializerLocator(t, factory);
        }
    }
}