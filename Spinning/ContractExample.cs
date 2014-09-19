using System;
using TheTunnel;

namespace Spinning
{
	[ProtoBuf.ProtoContract]
	public class mymsg{
		[ProtoBuf.ProtoMember(1)] public string Sender{get;set;}
		[ProtoBuf.ProtoMember(2)] public string Message{ get; set;}
		[ProtoBuf.ProtoMember(3)] public DateTime SendTime{get;set;}
	}
	public class ReceiverContract:ICordContract
	{
		[Receive("qmsg")]
		public void HandleMessage(mymsg msg)
		{
			Console.WriteLine ("[" + msg.SendTime + "] " + msg.Sender + ": " + msg.Message);
		}

		public event Action<ICordContract> PleaseDisconnect;

		public void OnDisconnect (DisconnectCause сause)
		{
			throw new NotImplementedException ();
		}
	}
	public class SenderContract: ICordContract
	{
		[Send("qmsg")]
		public Action<mymsg> SendMsg{get;set;}



		public event Action<ICordContract> PleaseDisconnect;
		public void OnDisconnect (DisconnectCause сause)
		{
			throw new NotImplementedException ();
		}

	}


	public class CordContractExample: ICordContract{

		[Send("SUSI")]
		//Call remote method
		public Action<U_S_E_R_I_N_F_O> SetUserInfo{ get; set;}

		[Ask(questionCordName: "qSUI", answerCordName: "aSUI")]
		//Call remote method 
		public Func<int, U_S_E_R_I_N_F_O> GetLasUserInfo{ get; set;}

		[Receive("STXT")]
		public void SetText(string text){
			//...Handling remote message here
		}

		[Answer(questionCordName: "qGUI", answerCordName: "aGUI")]
		public U_S_E_R_I_N_F_O GetUserInfo(int id){
			//...Handling remote call here
			return new U_S_E_R_I_N_F_O(){ Name = "Bzingo", Id = id} ;
		}
			

		//Wrapper of remote method call
		public U_S_E_R_I_N_F_O GetLastUserInfoWrapper(int id){
			return GetLasUserInfo (id);
		}

		public void OnDisconnect (DisconnectCause сause){
			Console.WriteLine (":( Connection was closed");
		}

		public event Action<ICordContract> PleaseDisconnect;
	}

	[ProtoBuf.ProtoContract]
	public class U_S_E_R_I_N_F_O{
		[ProtoBuf.ProtoMember(1)] public string Name{get;set;}
		[ProtoBuf.ProtoMember(2)] public string Surname{ get; set;}
		[ProtoBuf.ProtoMember(3)] public int Id{get;set;}
	}
}

