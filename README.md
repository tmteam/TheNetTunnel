# README #


### What is "TheTunnel" for? ###

It is a .Net implementation of lightweight data presentation/transmition protocol.

This protocol is designed for a using over TCP tranport.

  TheTunnel Provides:
*  Intuitive interface 
*  Large(or HUGE) data - packets delivery (more than 10 mb).
*  Simultaneous delivery of multiple data-packets
  	 (you can send a several files and whrite short message to client simultaneous, via one tcp/port for example)
*  Immitation for Remote method call. You can call method on other side and wait for its returns
*  Work with protobuf at serializing/desirializing data.
*  Functionality for easy definding your own message type, with Protobuf or build-in data types

## .net example ##

Simple chat implementation on TT:

~~~c#
//////////////////
//[Client-side]//
////////////////
	
public class ClientContract{
	[Out(1)] public Func<string,string,bool> SendMessage{get;set;}

	[In(2)]  public void ReceiveMessage(DateTime timeStamp, string sender, string message){
			Console.WriteLine ("["+timeStamp.ToShortTimeString()+"]" + sender + ": " + message);
	}
}
	
static void Main(string[] args)
{
	TcpClientTunnel client = new TcpClientTunnel ();
	ClientContract contract = new ClientContract ();
		
	Console.WriteLine ("Connecting to 172.16.31.34..");
	client.Connect (new IPAddress (new byte[]{ 172, 16, 31, 34 }), 1234, contract);
	Console.WriteLine ("Succesfully to connected!");

	while (true) {
		var msg = Console.ReadLine ();
	        contract.SendMessage ("tmt", msg);
	}
}
	
	
///////////////////
//[Server-side]///
/////////////////
	
public class ServerContract{
	[In(1)]  public bool MsgFromClient(string nick, string message){
		Console.WriteLine ("["+timeStamp.ToShortTimeString()+"] " + nick + ": " + message);
		return true; //we are always happy to take a message
	}
		
	[Out(2)] public Action<DateTime,string,string> SndMsg{get;set;}
}
	
static void Main(string[] args)
{
	TcpServerTunnel<ServerContract> server = new TcpServerTunnel<ServerContract>();
	
	server.OpenServer (IPAddress.Any, 1234);
	
	server.OnConnect+= (srv, contract) => {
		Console.WriteLine("Client connected"); 
		contract.SndMsg(DateTime.Now,"serv","Welcome to the tunnel");
	};
	
	while (true) {
		var msg = Console.ReadLine ();
		foreach (var contract in server.Contracts)
			contract.SndMsg (DateTime.Now, "serv", msg);
	}
}
	
	
~~~

Clear, isn't it?

In case of this primitive chat, protocol implementation is also very clear:

~~~

 1) SendMessageToServer  timestamp:UTC nick:UTF16    message:UTF16
 
 2) SendMessageToClient  nick:UTF16  message:UTF16 -> INT8 //will not lie to yourself - bool is always byte

~~~


## also ##
     
  Currently i'm testing convenience of .Net implementation and specifying serialization and deserialization format in some complex cases. If someone is interested in this kind of protocol, or wanna participate in its development - please let me know.
