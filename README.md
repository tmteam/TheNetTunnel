# README #


### What is "TheTunnel" for? ###

It is an implementation of flexible session + presentation-level protocol on .Net (mono)

This protocol is designed for a using via  duplex transport level protocol with guaranteed delivery. 
  Transport level protocol witch are using by theTunnel can be streaming-type. Actualy TheTunnel was tested only on TCP/IP at currently moment

  TheTunnel Provides:
  
*  Large(or HUGE) data - packets delivery (more than 10 mb).
*  Working va TCP/Ip protocol
*  Simultaneous delivery of multiple data-packets
  	 (you can send a several files and whrite short message to client simultaneous, via one tcp/port for example)
*  Immitation for Remote method call. You can call method on other side and wait for its returns
*  Work with protobuf at serializing/desirializing data.
*  Functionality for easy definding your own message type, with  
  Protobuf or your own Serialize and Deserialize method for each messageType.

## .net example ##

Simple dialog implementation:

~~~c#
  	public class CordContractExample: ICordContract{
		[InCord("STXT")]
		public void SetText(string text){
			//...Handling remote message here
		}

		[InCord("G_UI")]
		public UserInfo GetUserInfo(int id){
			//...Handling remote call here
			return new UserInfo(){ Name = "Bzingo", Id = id} ;
		}

		[OutCord("R_UI")]
		//Call remote method
		public Func<int, UserInfo> GetLasUserInfo{ get; set;}

		//Wrapper of remote method call
		public UserInfo GetLastUserInfoWrapper(int id){
			return GetLasUserInfo (id);
		}
		//Calling at disconnection
		public void OnDisconnect (DisconnectCause сause){
			Console.WriteLine (":( Connection was closed");
		}
		//Raise it, when you wanna close a connection
		public event Action<ICordContract> PleaseDisconnect;
	}

	[ProtoBuf.ProtoContract]
	public class UserInfo{
		[ProtoBuf.ProtoMember(1)] public string Name{get;set;}
		[ProtoBuf.ProtoMember(2)] public string Surname{ get; set;}
		[ProtoBuf.ProtoMember(3)] public int Id{get;set;}
	}
~~~

## also ##
     
* Currently this project is in alpha state, so i've got no help or any examples. 

*  This project shoud be done in a mounth because of job projects.

*  Specification of TheTunnel protocol will be posted later.

*  C - version of this protocol implementation, also will be posted later.

 If someone is interested in this kind of protocol - please let me know.
