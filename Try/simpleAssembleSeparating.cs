using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using TheTunnel;
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
            var mtp = new qSeparator();
            var res = mtp.Separate(arr, maxQSize, 123456789);
            qReceiver rec = new qReceiver();
            rec.OnMsg += rec_OnMsg;
			rec.OnError += rec_OnError;
            foreach (var r in res)
            {
				rec.Set(r);
            }
			if (!handled) 
				throw new Exception ("message loosed :(");
			if(hasError)
				throw new Exception ("handle error");
        }
        bool handled = false;
		bool hasError = false;
		void rec_OnError(qReceiver snd, qHead head, qReceiveError err)
		{
			hasError = true;
			Console.WriteLine ("GotError " + err);
		}
        void  rec_OnMsg(qReceiver arg1, qMsg arg2)
        {
            handled = true;
        }
    }

    

}
