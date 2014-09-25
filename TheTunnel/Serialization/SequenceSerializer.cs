using System;
using System.Collections.Generic;
using System.Linq;
namespace TheTunnel
{
	public class SequenceSerializer: ISerializer<object[]>
	{
		public readonly Type[] Types;
		ISerializer[] serializers;

		public SequenceSerializer (Type[] types)
		{
			this.Types = types;
			serializers = new ISerializer[types.Length];
			for (int i = 0; i < types.Length; i++) 
				serializers [i] = SerializersFactory.Create (types [i]);
			Size = null;
		}

		public bool TrySerialize (object[] obj, int offset, out byte[] arr)
		{
			arr = null;
			if (obj.Length != serializers.Length)
				return false;
			List<byte[]> res = new List<byte[]> ();
			res.Add (new byte[offset]);
			for(int i = 0; i< obj.Length; i++)
			{
				if (serializers [i].Size.HasValue) {
					byte[] ser;
					if (serializers [i].TrySerialize (obj[i], 0, out ser))
						res.Add (ser);
					else
						return false;
				}
				else {
					byte[] ser;
					if (serializers [i].TrySerialize (obj[i], 4, out ser)) {
						BitConverter.GetBytes (ser.Length - 4).CopyTo (ser, 0);
						res.Add (ser);
					}
					else
						return false;
				}
			}
			arr = res.SelectMany (r => r).ToArray ();
			return true;
		}

		public bool TrySerialize (object[] obj, byte[] arr, int offset)
		{
			if (obj.Length != serializers.Length)
				return false;
			int totalLength = offset;
			for(int i = 0; i< obj.Length; i++)
			{
				var seri = serializers [i];
				if (seri.Size.HasValue) {
					if (seri.Size.Value + totalLength > arr.Length)
						return false;
					if (seri.TrySerialize(obj[i], arr, totalLength))
						totalLength += serializers [i].Size.Value;
					else
						return false;
				}
				else {
					byte[] buff;
					if (seri.TrySerialize (obj[i], 4, out buff)) {
						if (buff.Length + totalLength > arr.Length)
							return false;
						BitConverter.GetBytes (buff.Length - 4).CopyTo (buff, 0);
						Array.Copy (buff, 0, arr, totalLength, buff.Length);
						totalLength += buff.Length;
					}
					else
						return false;
				}
			}
			return true;
		}

		public byte[] Serialize (object[] obj, int offset)
		{
			byte[] ans;
			if (TrySerialize (obj, offset, out ans))
				return ans;
			else
				return null;
		}

		public bool TrySerialize (object obj, int offset, out byte[] arr)
		{
			var objarr = obj as object[];
			if (objarr == null) {
				arr = null;
				return false;
			}
			return TrySerialize (objarr, offset, out arr);
		}

		public bool TrySerialize (object obj, byte[] arr, int offset)
		{
			var objarr = obj as object[];
			if (objarr == null) {
				return false;
			}
			return TrySerialize (objarr,  arr, offset);
		}

		public byte[] Serialize (object obj, int offset)
		{
			var objarr = obj as object[];
			if (objarr == null)
				return null;
			else
				return Serialize (objarr, offset);
		}

		public int? Size { get ; protected set; }
	}
}

