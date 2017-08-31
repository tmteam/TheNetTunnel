# TheNetTunnel
Tcp Client Server rpc library with binary serialization. Protobuf and or custom serialization support.
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
