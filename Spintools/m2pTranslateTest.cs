using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WhAlpaTest
{

    public class m2pTranslateTest
    {
        public bool Test()
        {
            for(ushort i = 20; i< 10001; i++)
            {
                Random rnd = new Random(DateTime.Now.Millisecond);

                var size = rnd.Next() % 10000;

                byte[] tstArr = new byte[size];
                rnd.NextBytes(tstArr);
                if (!GoodIOTest(tstArr, i))
                    return false;
            }
            return true;
        }
        bool GoodIOTest(byte[] arr, ushort maxQSize )
        {
            handled = false;
            var mtp = new whQuantumsGenerator();
            var res = mtp.Translate(BitConverter.ToInt32(Encoding.ASCII.GetBytes("ROPE"),0), arr, maxQSize, 123456789);
            whReceiver rec = new whReceiver();
            rec.OnMsg += rec_OnMsg;
            foreach (var r in res)
            {
                if (!rec.Set(r))
                {
                    Console.WriteLine("Unhandled :(");
                        return false;
                }
            }
            return handled;
        }
        bool handled = false;
        void  rec_OnMsg(whReceiver arg1, whMsg arg2)
        {
            handled = true;
        }
    }

    public class senderQueueTest
    {
        public bool Test()
        {
            msgdone = 0;
            whSender sender = new whSender()
            {
                MaxQuantumSize = 300,
            };

            int concurentMessagesCount = 10;
            for (int i = 0; i < concurentMessagesCount; i++)
            {
                var msg = new byte[1000];
                for (int j = 0; j < 1000; j++)
                {
                    msg[j] = (byte)(i * 10 + j % 10);
                }
                sender.Send(i, msg);
            }
            whReceiver receiver = new whReceiver();
            receiver.OnMsg += new Action<whReceiver, whMsg>(receiver_OnMsg);
            int chanid = 0;
            byte[] nmsg;
            while(sender.Next(out nmsg, out chanid))
            {
                receiver.Set(nmsg);
            }
            if (msgdone != concurentMessagesCount)
                return false;
            return sender.Lenght==0;
        }
        int msgdone = 0;
        void receiver_OnMsg(whReceiver arg1, whMsg arg2)
        {
            msgdone++;
            Console.WriteLine("msgGet: "+  arg2.cord+" : " + arg2.id+" : "+ arg2.Arr.Count(a=>a!=0));
        }
    }

}
