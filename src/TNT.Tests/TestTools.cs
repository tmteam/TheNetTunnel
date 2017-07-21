using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TNT.Tests
{
    public static class TestTools
    {
        public static Task AssertNotBlocks(Action  action, int maxTimeout = 1000)
        {
            var task = Task.Factory.StartNew(action);
            Assert.IsTrue(task.Wait(maxTimeout), "call is blocked");
            return task;

        }
        public static Task<T> AssertNotBlocks<T>(Func<T> func, int maxTimeout = 1000)
        {
            var task = Task.Factory.StartNew(func);
            Assert.IsTrue(task.Wait(maxTimeout), "call is blocked");
            return task;

        }
        public static void AssertThrowsAndNotBlocks<TException>(Action action)
        {
            var task = AssertTryCatchAndTaskNotBlocks(action);
            Assert.IsInstanceOf<TException>(task.Result);
        }

        public static Task<Exception> AssertTryCatchAndTaskNotBlocks(Action action)
        {
          return AssertNotBlocks(
             ()=> {   try
                {
                    action();
                }
                catch (Exception e)
                {
                    return e;
                }
                return null;
            });
        }
    }
}
