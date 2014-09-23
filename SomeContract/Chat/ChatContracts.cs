using System;
using TheTunnel;

namespace SomeContract
{
	public class ChatClientContract: IDisconnectable, IDisconnectListener
	{
		[In(2)]
		public bool ReceiveMsg(Msg message)
		{
			Console.WriteLine (message.Timestamp.ToShortTimeString () + " [" + message.User.Nick + "] " + message.Message);
			return true;
		}
		[In(3)]
		public void BeforeDisconnect(DisconnectInfo reason)
		{
			Console.WriteLine ("DISCONNECTED ["+reason.Reason+"] by "+reason.Initiator.Nick+" message: \""+ reason.Message);
		}

		[Out(1)] public Func<UserInfo, RegistrationResults> RegistrateMe{ get; set;}
		[Out(2)] public Func<Msg, bool> SendMessage{get; set;}
		[Out(4)] public Func<string, int[]> Ask4UserList{ get; set;}

		public void RaiseDisconnection()
		{
			if (DisconnectMe != null)
				DisconnectMe (this);
		}
		#region IDisconnectable implementation

		public event Action<IDisconnectable> DisconnectMe;

		#endregion

		#region IDisconnectListener implementation

		public void OnDisconnect (TheTunnel.DisconnectReason reason)
		{
			Console.WriteLine ("I was heavy Disconnected by reason: " + reason);
		}
		#endregion
	}

	public class ChatServerContract:IDisconnectable, IDisconnectListener
	{
		public ChatServerContract()
		{
			User = new UserInfo {
				FullName = "N/A",
				Nick = "unknown"
			};
		}

		public UserInfo User{get; protected set;}
		[In(1)]
		public RegistrationResults RegistrateMe(UserInfo User){
			Console.WriteLine ("Registration ask from " + User.Nick + " (" + User.FullName + ")");
			this.User = User;
			return new RegistrationResults{ Result = true, TimeStamp = DateTime.Now};
		}

		[In(2)]
		public bool OnReceiveMsg(Msg message){
			if (OnMessage != null)
				OnMessage (this, message);
			return true;
		}

		[In(4)]
		public int[] GetUserList(string reason)
		{	Console.WriteLine ("Asking for user list with reason: "+ reason);
			return new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 42 };
		}

		[Out(3)] public Action<DisconnectInfo> DisconnectUser{ get; set;}
		[Out(2)] public Func<Msg,bool> SendMessage{get; set;}

		public event Action<ChatServerContract, Msg> OnMessage;

		public void RaiseDisconnection()
		{
			if (DisconnectMe != null)
				DisconnectMe (this);
		}

		#region IDisconnectable implementation

		public event Action<IDisconnectable> DisconnectMe;

		#endregion

		#region IDisconnectListener implementation

		public void OnDisconnect (TheTunnel.DisconnectReason reason)
		{
			Console.WriteLine ("Client was Disconnected by reason: " + reason);
		}

		#endregion
	}
}

