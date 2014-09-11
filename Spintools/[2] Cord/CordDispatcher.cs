using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace TheTunnel
{
	public class CordDispatcher
	{
		public CordDispatcher (){
			cords = new Dictionary<string, ICord> ();
		}

		public string[] Names{ get; protected set; }

		public ICord GetCord(string name){
			return cords [name];
		}

		public void RemoveCord(string name)
		{
			cords.Remove (name);
		}

		public void AddCord(ICord cord)
		{
			if (!cords.ContainsKey (cord.Name))
				cords.Add (cord.Name, cord);
			else {
				cords [cord.Name].Need2Send -= cord_NeedSend;
				cords [cord.Name] = cord;
			}
				cord.Need2Send += cord_NeedSend;

			var askcord = cord as IAskingCord;
			if (askcord != null)
				AddCord (askcord.AnswerCord);
		}
	
		public void Parse(byte[] qMsg)
		{
			string name = Encoding.ASCII.GetString (qMsg, 0, 4);
			if (cords.ContainsKey (name))
				cords [name].Handle (qMsg);
		}

		public event Action<CordDispatcher, byte[]> NeedSend;

		Dictionary<string,ICord> cords;

		void cord_NeedSend(ICord sender,byte[] qMsg)
		{
			if(NeedSend!=null)
				NeedSend(this, qMsg);
		}
	}
}

