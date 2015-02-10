using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TheTunnel.Light
{
    /// <summary>
    /// Collect single light message from quants
    /// </summary>
	public class LightCollector
	{
		static int DefaultHeadSize = Marshal.SizeOf(typeof(QuantumHead));

		public DateTime lastTS;

		MemoryStream stream = null;

		int lenght = 0;
        
        /// <summary>
        /// Handles new portion of data from a transport
        /// </summary>
        /// <param name="head">Quant head object</param>
        /// <param name="packetFromAStream">bytes from a transport stream</param>
        /// <param name="headStart">position, where head starts</param>
        /// <returns>true if light-message is fully collected</returns>
		public bool Collect(QuantumHead head, byte[] packetFromAStream, int headStart)
		{
			lastTS = DateTime.Now;
			int bodyStart = headStart + DefaultHeadSize;
			int bodyLen = head.length - DefaultHeadSize;
			if(stream == null)
			{
				if (head.type == QuantumType.Start) {
					lenght= BitConverter.ToInt32 (packetFromAStream, bodyStart );
					stream = new MemoryStream (lenght);
					stream.Write (packetFromAStream, bodyStart + 4, bodyLen - 4); 
				} else//Stream is null and its mean Error
					return true;
			}
			else if (head.type == QuantumType.Data) {
				stream.Write (packetFromAStream, bodyStart, bodyLen); 
			} else {
				stream = null;
				return true;
			}
			if (stream.Length == lenght)
				return true;
			if (stream.Length < lenght)
				return false;

			stream = null;
			return true;
		}
        /// <summary>
        /// reset collector
        /// </summary>
		public void Clear(){
			stream = null;
			lenght = 0;
		}
        /// <summary>
        /// Get collected message stream
        /// </summary>
        /// <returns></returns>
		public MemoryStream GetLightMessageStream(){
			return stream;
		}
	}
}
