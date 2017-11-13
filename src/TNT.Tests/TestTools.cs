using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TNT.Exceptions.Remote;

namespace TNT.Tests
{
    public static class TestTools
    {
        public static void AssertTrue(Func<bool> condition, int maxAwaitIntervalMs, string message = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!condition())
            {
                if (sw.ElapsedMilliseconds > maxAwaitIntervalMs)
                {
                    Assert.Fail(message);
                    return;
                }
                Thread.Sleep(1);
            }
        }
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

        public static TException AssertThrows_AndNotBlocks_AndContainsInfo<TException>(Action action) where TException : RemoteException
        {
            var exception = AssertThrowsAndNotBlocks<TException>(action);
            Assert.False(exception.Message.Contains(RemoteException.DefaultExceptionText), "Additional exception information missed");
            var task = AssertTryCatchAndTaskNotBlocks(action);
            Assert.IsInstanceOf<TException>(task.Result);
            return (TException)(object)task.Result;
        }

        public static TException AssertThrowsAndNotBlocks<TException>(Action action) where TException: Exception
        {
            var task = AssertTryCatchAndTaskNotBlocks(action);
            Assert.IsInstanceOf<TException>(task.Result);
            return (TException) (object) task.Result;
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
