# TheNetTunnel
Tcp Client Server rpc library with binary serialization. Protobuf and or custom serialization is supporting out-of-box.

TNT is supposed for being fast compact and easy to use library. Watch speed test results below.   

To install TNT, run the following command in the Package Manager Console:

```
PM> Install-Package tnt
```

# Example
```
void main(){
 var server = TntBuilder
      .UseContract<IExampleContract, ExampleContract>()
      .CreateTcpServer(IPAddress.Any, 12345);
  server.StartListening();

  Console.WriteLine("Type your messages:");
  while (true) {
      var message = Console.ReadLine();
      using (var client = TntBuilder.UseContract<IExampleContract>()
          .CreateTcpClientConnection(IPAddress.Loopback, 12345))
      {
         client.Contract.Send("Superman", message);
      }  
  }
  server.Close();
}

//contract
public interface IExampleContract     {
    [TntMessage(1)] void Send(string user, string message);
}
//contract implementation
public class ExampleContract : IExampleContract     {
    public void Send(string user, string message)         {
        Console.WriteLine($"[Server received:] {user} : {message}");
    }
}
```

Other examples: 

- [Stage 1. Easy start](https://github.com/tmteam/TheNetTunnel/blob/master/src/Example/Stage1_EasyStart/Stage1_EasyStartExample.cs)
- [Stage 2. Diehard example](https://github.com/tmteam/TheNetTunnel/blob/master/src/Example/Stage2_ComplexExample/Stage2_Example.cs)
- [Stage 3. Let's test!](https://github.com/tmteam/TheNetTunnel/blob/master/src/Example/Stage3_IntroducingToTestingExample/Stage3_Example.cs)

# Speed test results

It's hard to name a single figure, otrageous overall TNT speed if you're not a marketer. There are many such numbers.
All results are performed by [local speed test](https://github.com/tmteam/TheNetTunnel/tree/master/src/TNT.SpeedTest) 
```

Result speed: 2.5 - 5 Gbit (no serialization), 0.4 - 1 GBit (protobuff serialization) (see below) 
Output localhost Delay: 9 microseconds
Output message overhead: 6 byte per message
Echo transaction localhost (IO) Delay: 34,87 microseconds
Echo transaction (I/O) overhead : 8/12 byte per message


Output speed:
[Sending in "fire and forget" style]

Raw byte Array:       up to 2300 megabytes/sec
String serialization: up to 830 megabytes/sec
Protobuff serialization: up to 120 megabytes/sec

Echo transaction speed:
[Send data and received its copy]

Raw byte Array:       up to 300 megabytes/sec
String serialization: up to 188 megabytes/sec
Protobuff serialization: up to 47 megabytes/sec


Overheads:

Output localhost Delay: 9,06 microseconds
Output message overhead: 6 byte per message

Echo transaction localhost (IO) Delay: 34,87 microseconds
Echo transaction (I/O) overhead : 8/12 byte per message
```

