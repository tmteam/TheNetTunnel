using System;

namespace TNT.Exceptions
{
    public class TypeCannotBeDeserializedException : Exception
    {
        private readonly Type _t;

        public TypeCannotBeDeserializedException(Type t) :
            base("Type " + t.Name + " cannot be deseserialized.")
        {
            _t = t;
        }
    }
}