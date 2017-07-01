using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amib.Threading;
using Action = System.Action;

namespace TNT
{
    public static class TNTThreadPool
    {
        static SmartThreadPool _poolsingletone ;
        static TNTThreadPool()
        {
            _poolsingletone = new SmartThreadPool(100, 1000);
        }

        public static void Run(Action action)
        {
            _poolsingletone.QueueWorkItem(()=>action());
        }
    }
}
