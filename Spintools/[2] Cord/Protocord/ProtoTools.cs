﻿using System;
using System.IO;
using System.Runtime.Serialization;

namespace TheTunnel
{
	public static class ProtoTools
	{
		public static byte[] Serialize<T> (T msg,  int valOffset)
		{
			using (var stream = new MemoryStream())
			{
				stream.Position = valOffset;
				ProtoBuf.Serializer.Serialize<T> (stream, msg);
				var lenght = (int)stream.Length;
				var buff = stream.GetBuffer ();
				Array.Resize (ref buff, lenght);
				return buff;
			}
		}

		public static bool TryDeserialize<T> (byte[] qMsg, int offset, out T msg)
		{
			using (var stream = new MemoryStream(qMsg,offset, qMsg.Length-offset))
			{
				try
				{
					msg = ProtoBuf.Serializer.Deserialize<T>(stream);
					return true;
				}
				catch(Exception ex) {
					msg = default(T);
					return false;
				}
			}
		}

	}
}

