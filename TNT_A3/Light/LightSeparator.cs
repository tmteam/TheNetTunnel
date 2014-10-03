	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Runtime.InteropServices;
	using System.IO;

	namespace TheTunnel
	{
		public class LightSeparator 
		{
			static int DefaultHeadSize = Marshal.SizeOf(typeof(QuantumHead));

			public void Initialize(Stream stream,  int msgId)
			{
				left = (int)(stream.Length - stream.Position);
				this.msgId = msgId;
				got1Sended = false;
			}

			int  left;
			public int Left{ get{ return left; }}

			int msgId;
			public int MsgId{ get { return msgId; } }

			Stream currentStream;
			bool got1Sended = false;

			public byte[] Next(int maxQuantSize)
			{
				left = (int)(currentStream.Length - currentStream.Position);
				var actualHeadSize = got1Sended ? DefaultHeadSize : (DefaultHeadSize + 4);
				var head = new QuantumHead {
					length = (ushort)Math.Min(actualHeadSize+left,maxQuantSize),
					msgId = msgId,
					type = got1Sended ? QuantumType.Data : QuantumType.Start,
				};

				byte[] Quant = new byte[head.length];

				//Head
				head.SetToArray (Quant, 0, DefaultHeadSize);

				//Length if start quant
				if (!got1Sended)
					Array.Copy(BitConverter.GetBytes(left),0,Quant,DefaultHeadSize,4);

				//data
				currentStream.Read (Quant, actualHeadSize, head.length-actualHeadSize);

				got1Sended = true;
				return Quant;
			}
		}
	}


