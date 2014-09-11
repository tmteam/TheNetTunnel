using System;
using System.Text;
using System.Runtime.InteropServices;

namespace TheTunnel
{
	public class SayUTF32: SayingCordBase<string>
	{
		public SayUTF32(string cordName):base(cordName){}

		protected override byte[] Serialize (string msg, int msgOffset)
		{
			byte[] ans = new byte[msg.Length*4 + msgOffset];
			Encoding.UTF32.GetBytes(msg,0, msg.Length, ans, msgOffset);
			return ans;
		}

		protected override bool TryDeserialize (byte[] qMsg, int offset, out string msg)
		{
			msg = null;
			//Check utf32 covertion possibility
			if ((qMsg.Length - offset) % 4 != 0)
				return false;
			else {
				msg =  Encoding.UTF32.GetString (qMsg, offset, qMsg.Length - offset);
				return true;
			}
		}
	}

	public class SayASCII: SayingCordBase<string>
	{
		public SayASCII(string cordName):base(cordName){}

		protected override byte[] Serialize (string msg, int msgOffset)
		{
			byte[] ans = new byte[msg.Length + msgOffset];
			Encoding.ASCII.GetBytes(msg,0, msg.Length, ans, msgOffset);
			return ans;
		}

		protected override bool TryDeserialize (byte[] qMsg, int offset, out string value)
		{
			value = Encoding.ASCII.GetString (qMsg, offset, qMsg.Length - offset);
			return true;
		}
	}

	public class SayFixedSizeObject<T>: SayingCordBase<T>
	{
		int objectSize;
		public SayFixedSizeObject(string cordName):base(cordName)
		{
			objectSize = Marshal.SizeOf(typeof(T));
		}

		protected override byte[] Serialize (T val, int msgOffset)
		{
			byte[] ans = new byte[msgOffset + objectSize];
			Tools.SetToArray (val, ans, msgOffset, objectSize);
			return ans;
		}

		protected override bool TryDeserialize (byte[] qMsg, int offset, out T msg)
		{
			if (qMsg.Length < offset + objectSize) {
				msg = default(T);
				return false;
			} else {
				msg = Tools.ToStruct<T> (qMsg, offset, objectSize);
				return true;
			}
		}
	}
}

