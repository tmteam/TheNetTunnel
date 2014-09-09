using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using WhAlpaTest;
using NUnit.Framework;

namespace Try
{
	[TestFixture ()]
    public class simpleAssembleSeparating
    {
		[Test ()]
		public void Test()
        {
			for(ushort i = 20; i< 10001; i++)
            {
                Random rnd = new Random(DateTime.Now.Millisecond);

                var size = rnd.Next() % 10000;

                byte[] tstArr = new byte[size];
                rnd.NextBytes(tstArr);
				GoodIOTest (tstArr, i);
            }
          
        }

		void GoodIOTest(byte[] arr, ushort maxQSize )
        {
            handled = false;
            var mtp = new whQuantumsGenerator();
            var res = mtp.Translate(arr, maxQSize, 123456789);
            whReceiver rec = new whReceiver();
            rec.OnMsg += rec_OnMsg;
            foreach (var r in res)
            {
                if (!rec.Set(r))
                {
                    Console.WriteLine("Unhandled :(");
					throw new Exception ("Unhandled :(");
                }
            }
			if (!handled) 
				throw new Exception ("message loosed :(");
        }
        bool handled = false;
        void  rec_OnMsg(whReceiver arg1, whMsg arg2)
        {
            handled = true;
        }
    }

    

}
