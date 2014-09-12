# README #

*===========================
*...........................   
*..........\\....//.........
*.....______.....______.....
*....||. . .\\.//. . .||....
*....||. . .||.||. . .||....
*....||. . .||.||. . .||....
*....||. . .||.||. . .||....
*....||____//...\\____||.... 
*........................... 
* ==========================


### What is "TheTunnel" for? ###

It is an implementation of flexible session + presentation-level protocol on .Net (mono)

This protocol is designed for a using via  duplex transport level protocol with guaranteed delivery. 
  Transport level protocol witch are using by theTunnel can be streaming-type. Actualy TheTunnel was tested only on TCP/IP at currently moment

  TheTunnel Provides:
  	-  Large(or HUGE) data - packets delivery (more than 10 mb).
  	-  Working va TCP/Ip protocol
  	-  Simultaneous delivery of multiple data-packets
  	 (you can send a several files and whrite short message to client simultaneous, via one tcp/port for example)
  	-  Immitation for Remote method call. You can call method on other side and wait for its returns
  	-  Work with protobuf at serializing/desirializing data.
  	-  Functionality for easy definding your own message type, with  
  Protobuf or your own Serialize and Deserialize method for each messageType.
        
     
Currently this project is in alpha state, so i've got no help or any examples. 

 This project shoud be done in a mounth because of job projects.

 Specification of TheTunnel protocol will be posted later.

 C - version of this protocol implementation, also will be posted later.

 If someone is interested in this protocol - please let me know.