using System;
using System.IO;
using System.Text;

namespace TNT.SpeedTest
{
    public class Output
    {
        private readonly StringBuilder _sb = new StringBuilder();
        public void WriteLine(string str)
        {
            Console.WriteLine(str);
            _sb.AppendLine(str);
        }
        public void WriteLine()
        {
            Console.WriteLine();
            _sb.AppendLine();
        }

        public void Clear()
        {
            _sb.Clear();
        }
        public bool TrySaveTo(string path)
        {
            try
            {
                File.WriteAllText(path, _sb.ToString());
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
