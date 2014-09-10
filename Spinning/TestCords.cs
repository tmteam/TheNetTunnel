using System;
using TheTunnel;
using System.Text;

namespace Spinning
{
	class MsgWithA:AskingCordBase<string,string>{
		protected override string Answer (string question){
			return "Getted: " + question;
		}

		protected override ISayingCord<string> CreateAnswerCord (){
			return new MsgCordTyped ();
		}

		protected override string GetName (){
			return "qsas";
		}

		protected override byte[] Serialize (string val){
			return Encoding.ASCII.GetBytes (val);
		}

		protected override string Deserialize (byte[] qMsg){
			return Encoding.ASCII.GetString (qMsg,4,qMsg.Length-4);
		}
	}


	class MsgCordTyped:SayingCordBase<string>
	{
		#region implemented abstract members of SayCordBase
		protected override string GetName ()
		{
			return "smsg";
		}

		protected override byte[] Serialize (string val)
		{
			return Encoding.ASCII.GetBytes (val);
		}

		protected override string Deserialize (byte[] qMsg)
		{
			return Encoding.ASCII.GetString (qMsg,4,qMsg.Length-4);
		}

		#endregion
	}
	class HiCord:SayingCordBase
	{
		#region implemented abstract members of SayCordBase
		protected override string GetName ()
		{
			return "hi:)";
		}
		public override bool Parse (byte[] cord)
		{
			if (cord.Length == 4) {
				Console.WriteLine ("get hi");
				return true;
			} else {
				Console.WriteLine ("wtf");
			}
			return false;

		}
		public void SayHi()
		{
			SendNeedSend (new byte[0]);
		}
		#endregion
	}
	class MsgCord:SayingCordBase
	{
		#region implemented abstract members of SayCordBase
		protected override string GetName ()
		{
			return "smsg";
		}
		public override bool Parse (byte[] qMsg)
		{
			var msg = Encoding.ASCII.GetString (qMsg,4,qMsg.Length-4);
			Console.WriteLine ("HasMsg " + msg);
			return true;
		}
		public void SendMsg(string message){
			SendNeedSend (Encoding.ASCII.GetBytes (message));
		}
		#endregion
	}
}

