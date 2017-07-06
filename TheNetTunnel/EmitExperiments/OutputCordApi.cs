using System;
using System.Collections.Generic;

namespace EmitExperiments
{
    public class OutputCordApi : IOutputCordApi
    {
        public void Say(int cordId, object[] values)
        {
            Console.WriteLine("Say. Arg.Count: " + values.Length);
            foreach (var value in values)
            {
                Console.WriteLine("   value: " + value);
            }
        }

        public T Ask<T>(int cordId, object[] values)
        {
            Console.WriteLine("Ask");
            foreach (var value in values)
            {
                Console.WriteLine("   value: " + value);
            }
            object ans = null;
            if (typeof(T) == typeof(int))
                ans = 42;
            else if (typeof(T) == typeof(string))
                ans = "fortytwo";
            else ans = default(T);

            return (T) ans;
        }

        public void Subscribe(int cordId, Delegate callback)
        {
        }

        public Dictionary<int, Func<object[], object>> AskSubScribed = new Dictionary<int, Func<object[], object>>();

        public Dictionary<int, Action<object[]>> SaySubScribed = new Dictionary<int, Action<object[]>>();

        public void SaySubscribe(int cordId, Action<object[]> callback)
        {
            SaySubScribed.Add(cordId, callback);
        }

        public void AskSubscribe<T>(int cordId, Func<object[], T> callback)
        {
            AskSubScribed.Add(cordId, (o) => callback(o));
        }

        public void Unsubscribe(int cordId)
        {
        }
    }
}