using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace TheTunnel
{
	public class CordDispatcher
	{
		public CordDispatcher ()
		{
			cords = new Dictionary<string, ISayingCord> ();
		}
		public string[] Names{ get; protected set; }
		public ISayingCord GetCord(string name){
			return cords [name];
		}

		public void RemoveCord(string name)
		{
			cords.Remove (name);
		}
		public void AddCord(ISayingCord cord)
		{
			if (!cords.ContainsKey (cord.Name))
				cords.Add (cord.Name, cord);
			else {
				cords [cord.Name].NeedSend -= cord_NeedSend;
				cords [cord.Name] = cord;
			}
				cord.NeedSend += cord_NeedSend;

			var askcord = cord as IAskingCord;
			if (askcord != null)
				AddCord (askcord.AnswerCord);
		}
		public void Parse(byte[] qMsg)
		{
			string name = Encoding.ASCII.GetString (qMsg, 0, 4);
			if (cords.ContainsKey (name))
			{
				cords [name].Parse (qMsg);
			}
		}

		public event Action<CordDispatcher, byte[]> NeedSend;

		Dictionary<string,ISayingCord> cords;

		void cord_NeedSend(ISayingCord sender,byte[] cordMsg)
		{
			byte[] qMsg = new byte[cordMsg.Length + 4];
			sender.BName.CopyTo (qMsg, 0);
			cordMsg.CopyTo (qMsg, 4);
			if(NeedSend!=null)
				NeedSend(this, qMsg);
		}
	}
}

