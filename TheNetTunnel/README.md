# README #


### What is "TheTunnel" for? ###

It is a .Net implementation of lightweight data presentation/transmition protocol.

This protocol is designed for a using over TCP tranportю

  TNT Provides:
  
*  Intuitive interface. 
*  Large packets guaranteed delivery (already tested on 20mb packets).
*  Simultaneous delivery of multiple data-packets
  	 (For example: you can send a several files and whrite short message to client simultaneous, via one tcp-port)
*  Immitation for Remote method call. You can call method on other side and wait for its returns in classical programm flow style)
*  in-box protobuff serialization.
*  Functionality for easy definding your own message type, with Protobuf or build-in data types

## .net example ##

Simple chat implementation on TT:

Client code:
~~~c#

public class ClientContract{
	[Out(1)] public Func<string,string,bool> SendMessage{get;set;}
	
	//You can use event like Action<DateTime,string,string>(or any other delegateType with same signature)
	//instead of direct method implementation here:
	[In(2)]  public void ReceiveMessage(DateTime time, string nick, string msg){
			Console.WriteLine ("["+time+"]" + nick + ": " + msg);
	}
}
	
static void Main(string[] args)
{
	var client = new TcpClientTunnel ();
	var contract = new ClientContract ();
		
	Console.WriteLine ("Connecting to 172.16.31.34..");
	client.Connect (new IPAddress (new byte[]{ 172, 16, 31, 34 }), 1234, contract);
	
	Console.WriteLine ("Succesfully to connected!");
	//Let's Chat!
	while (true) {
		var msg = Console.ReadLine ();
	        if(contract.SendMessage ("tmt", msg))
	        	Console.WriteLine("[message sended]");
	}
}
~~~
Server code:
~~~C#	

public class ServerContract{
	[In(1)]  public bool MsgFromClient(string nick, string message){
		Console.WriteLine (nick + ": " + message);
		return true; //we are always happy to receive a message
	}
		
	[Out(2)] public Action<DateTime,string,string> SndMsg{get;set;}
}
	
static void Main(string[] args)
{
	var server = new TcpServerTunnel<ServerContract>();
	
	server.OpenServer (IPAddress.Any, 1234);
	
	server.OnConnect+= (srv, contract) => {
		Console.WriteLine("Client connected"); 
		contract.SndMsg(DateTime.Now,"serv","Welcome to the tunnel");
	};
	//Chatting:
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

   1 VOID SendMessageToServer  UTC:timestamp  STRING:nick    STRING:message
 
 <-2 BYTE SendMessageToClient  STRING:nick    STRING:message  #will not lie to yourself - bool is always byte

~~~


## also ##
  
  This code already worked in production sience 21.10.14
  Currently i'm working on speed improvments in case of large number of messages per second.  
  If someone wanna to participate on its development - please let me know.
