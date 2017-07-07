using System;
using System.Runtime.InteropServices;

namespace TNT.Cord.Serializers
{
    public class SerializationRule
    {
        private readonly Predicate<Type> _rule;
        private readonly Func<Type, SerializerFactory, ISerializer> _serializerLocator;

        public SerializationRule(Predicate<Type> rule, Func<Type, SerializerFactory, ISerializer> serializerLocator)
        {
            _rule = rule;
            _serializerLocator = serializerLocator;
        }

        public SerializationRule(Predicate<Type> rule, Func<Type, ISerializer> serializerLocator)
        {
            _rule = rule;
            _serializerLocator = (t,f)=> serializerLocator(t);
        }

        public static SerializationRule Create<T>(ISerializer<T> serializer)
        {
            return new SerializationRule((t)=> t == typeof(T),(t)=>serializer);
        }

        public Predicate<Type> Rule
        {
            get { return _rule; }
        }

        public ISerializer GetSerializer(Type type, SerializerFactory factory)
        {
            return _serializerLocator(type, factory);
        }
    }
}