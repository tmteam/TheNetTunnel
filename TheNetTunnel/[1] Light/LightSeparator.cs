	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Runtime.InteropServices;
	using System.IO;

	namespace TheTunnel.Light
	{
        /// <summary>
        /// Separate lightMessage into sequence of quants
        /// </summary>
		public class LightSeparator 
		{
			static int DefaultHeadSize = Marshal.SizeOf(typeof(QuantumHead));

			int  dataLeft;
			public int DataLeft{ get{ return dataLeft; }}

			int msgId;
			public int MsgId{ get { return msgId; } }

			Stream currentStream;
			bool got1Sended = false;

			public void Initialize(Stream stream,  int msgId)
			{
				dataLeft = (int)(stream.Length - stream.Position);
				this.msgId = msgId;
				got1Sended = false;
				currentStream = stream;
			}

			public byte[] Next(int maxQuantSize)
			{
				dataLeft = (int)(currentStream.Length - currentStream.Position);
				var actualHeadSize = got1Sended ? DefaultHeadSize : (DefaultHeadSize + 4);
				var head = new QuantumHead {
					length = (ushort)Math.Min(actualHeadSize+dataLeft,maxQuantSize),
					msgId = msgId,
					type = got1Sended ? QuantumType.Data : QuantumType.Start,
				};

				byte[] Quant = new byte[head.length];

				//Head
				head.SetToArray (Quant, 0, DefaultHeadSize);

				//Length if start quant
				if (!got1Sended)
					Array.Copy(BitConverter.GetBytes(dataLeft),0,Quant,DefaultHeadSize,4);

				//data
				currentStream.Read (Quant, actualHeadSize, head.length-actualHeadSize);

				got1Sended = true;
				dataLeft -= (Quant.Length - actualHeadSize);
				return Quant;
			}
		}
	}


